// Controllers/OrdersController.cs
using LavandariaGaivotaAPI.Data;
using LavandariaGaivotaAPI.Dtos;
using LavandariaGaivotaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace LavandariaGaivotaAPI.Controllers
{
    [Authorize] // <<< Este atributo protege TODOS os endpoints neste controlador
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto createOrderDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // --- Obter o ID do utilizador autenticado a partir do token JWT ---
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                // Este erro não deveria acontecer se o atributo [Authorize] estiver a funcionar
                return Unauthorized("Não foi possível identificar o utilizador.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return Unauthorized("Utilizador inválido.");
            }

            // 1. Encontrar a morada selecionada na base de dados
            var selectedAddress = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == createOrderDto.AddressId && a.UserId == userId);

            // 2. Verificar se a morada foi encontrada e pertence ao utilizador
            if (selectedAddress == null)
            {
                return BadRequest(new { Message = "Morada inválida ou não pertence ao utilizador." });
            }

            // 3. Formatar a morada como uma única string para guardar no pedido
            string fullAddress = $"{selectedAddress.Street}, {selectedAddress.Number}" +
                                (!string.IsNullOrEmpty(selectedAddress.Floor) ? $", {selectedAddress.Floor}" : "") +
                                $", {selectedAddress.PostalCode} {selectedAddress.City}";


            // Mapear do DTO para o nosso modelo de dados, usando a morada formatada
            var newOrder = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pendente,
                PickupAddress = fullAddress, // <<< Usar a string da morada formatada
                Notes = createOrderDto.Notes,
                OrderItems = new List<OrderItem>(),
                TotalPrice = 0
            };

            foreach (var itemDto in createOrderDto.OrderItems)
            {
                var orderItem = new OrderItem
                {
                    ServiceDescription = itemDto.ServiceDescription,
                    Quantity = itemDto.Quantity,
                    UnitPrice = itemDto.UnitPrice
                };
                newOrder.OrderItems.Add(orderItem);
                newOrder.TotalPrice += itemDto.Quantity * itemDto.UnitPrice;
            }

            // Adicionar o novo pedido ao DbContext e guardar na base de dados
            try
            {
                await _context.Orders.AddAsync(newOrder);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log do erro (não implementado, mas importante para produção)
                Console.WriteLine($"Erro ao guardar o pedido: {ex.Message}");
                return StatusCode(500, "Ocorreu um erro interno ao criar o seu pedido.");
            }

            var orderResponse = new OrderResponseDto
            {
                Id = newOrder.Id,
                UserId = newOrder.UserId,
                UserFullName = user.FullName, // Assumindo que tem o objeto 'user' disponível
                OrderDate = newOrder.OrderDate,
                Status = newOrder.Status.ToString(), // Converte o enum para string
                TotalPrice = newOrder.TotalPrice,
                PickupAddress = newOrder.PickupAddress,
                Notes = newOrder.Notes,
                OrderItems = newOrder.OrderItems.Select(oi => new OrderItemResponseDto
                {
                    Id = oi.Id,
                    ServiceDescription = oi.ServiceDescription,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice
                }).ToList()
            };
            // Retorna uma resposta de sucesso com o pedido criado
            // (Poderia criar um OrderResponseDto para não expor o modelo de dados completo)
            return CreatedAtAction(nameof(GetOrderById), new { id = newOrder.Id }, orderResponse);
        }

        // Endpoint de exemplo para ir buscar um pedido por ID (para que o CreatedAtAction funcione)
        // Vamos implementá-lo de forma básica por agora.
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            // Garante que o utilizador só pode ver os seus próprios pedidos
            if (order.UserId != userId)
            {
                return Forbid(); // Ou NotFound() para não revelar a existência do pedido
            }

            return Ok(order);
        }

        // GET: api/orders - Obter o histórico de pedidos do utilizador autenticado
        [HttpGet]
        public async Task<IActionResult> GetUserOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return Unauthorized();

            var orders = await _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.OrderItems) // <<< IMPORTANTE: Inclui os itens de cada pedido
                .OrderByDescending(o => o.OrderDate) // Ordena pelos mais recentes primeiro
                .Select(order => new OrderResponseDto // Mapeia para o DTO para evitar ciclos
                {
                    Id = order.Id,
                    UserId = order.UserId,
                    UserFullName = user.FullName,
                    OrderDate = order.OrderDate,
                    Status = order.Status.ToString(),
                    TotalPrice = order.TotalPrice,
                    PickupAddress = order.PickupAddress,
                    Notes = order.Notes,
                    OrderItems = order.OrderItems.Select(oi => new OrderItemResponseDto
                    {
                        Id = oi.Id,
                        ServiceDescription = oi.ServiceDescription,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        }

        // GET: api/orders/all - Obter TODOS os pedidos (apenas Admin)
        [HttpGet("all")]
        [Authorize(Roles = "Admin")] // <<< Protegido por Role!
        public async Task<IActionResult> GetAllOrders([FromQuery] string? status)
        {
            // Começa com uma query base
            var query = _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .AsQueryable();

            // Se um status for fornecido e for válido, adiciona um filtro à query
            if (!string.IsNullOrEmpty(status))
            {
                // Tenta converter a string para o nosso enum OrderStatus
                if (Enum.TryParse<OrderStatus>(status, true, out var statusEnum))
                {
                    query = query.Where(o => o.Status == statusEnum);
                }
                else
                {
                    return BadRequest("O estado fornecido é inválido.");
                }
            }

            // O resto da query continua igual
            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Select(order => new OrderResponseDto // Mapeia para o DTO
                {
                    Id = order.Id,
                    UserId = order.UserId,
                    UserFullName = order.User!.FullName,
                    OrderDate = order.OrderDate,
                    Status = order.Status.ToString(),
                    TotalPrice = order.TotalPrice,
                    PickupAddress = order.PickupAddress,
                    Notes = order.Notes,
                    OrderItems = order.OrderItems.Select(oi => new OrderItemResponseDto
                    {
                        Id = oi.Id,
                        ServiceDescription = oi.ServiceDescription,
                        Quantity = oi.Quantity,
                        UnitPrice = oi.UnitPrice
                    }).ToList()
                })
                .ToListAsync();

            return Ok(orders);
        
        }

        // PUT: api/orders/{id}/status - Atualizar o estado de um pedido (apenas Admin)
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto statusDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound($"Pedido com ID {id} não encontrado.");
            }

            // Converte a string do DTO para o tipo enum
            if (Enum.TryParse<OrderStatus>(statusDto.Status, true, out var newStatus))
            {
                order.Status = newStatus;
                await _context.SaveChangesAsync();
                return Ok(new { Message = $"Estado do pedido {id} atualizado para {newStatus}." });
            }

            return BadRequest("Estado inválido fornecido.");
        }

        [HttpPut("{id}/cancel")]
        [Authorize] // Protegido, apenas o utilizador autenticado pode aceder
        public async Task<IActionResult> CancelOrder(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // Encontra o pedido e garante que pertence ao utilizador logado
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound("Pedido não encontrado ou não tem permissão para o alterar.");
            }

            // Regra de negócio: só pode cancelar se o estado for "Pendente"
            if (order.Status != OrderStatus.Pendente)
            {
                return BadRequest(new { Message = $"Não é possível cancelar um pedido com o estado '{order.Status}'." });
            }

            order.Status = OrderStatus.Cancelado; // Altera o estado
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Pedido #{id} cancelado com sucesso." });
        }
    
    
    }
}