// Controllers/AddressesController.cs
using LavandariaGaivotaAPI.Data;
using LavandariaGaivotaAPI.Dtos;
using LavandariaGaivotaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LavandariaGaivotaAPI.Controllers
{
    [Authorize] // Protege todos os endpoints neste controlador
    [Route("api/[controller]")]
    [ApiController]
    public class AddressesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AddressesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/addresses - Obter todas as moradas do utilizador autenticado
        [HttpGet]
        public async Task<IActionResult> GetUserAddresses()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var addresses = await _context.Addresses
                .Where(a => a.UserId == userId)
                .Select(a => new AddressDto // Mapear para o DTO
                {
                    Id = a.Id,
                    Alias = a.Alias,
                    Street = a.Street,
                    Number = a.Number,
                    Floor = a.Floor,
                    PostalCode = a.PostalCode,
                    City = a.City,
                    IsDefault = a.IsDefault
                })
                .ToListAsync();

            return Ok(addresses);
        }

        // POST: api/addresses - Adicionar uma nova morada
        [HttpPost]
        public async Task<IActionResult> AddAddress([FromBody] CreateOrUpdateAddressDto addressDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            // Se esta nova morada for definida como padrão, desmarcar as outras
            if (addressDto.IsDefault)
            {
                var currentDefault = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);
                if (currentDefault != null)
                {
                    currentDefault.IsDefault = false;
                }
            }

            var newAddress = new Address
            {
                UserId = userId,
                Alias = addressDto.Alias,
                Street = addressDto.Street,
                Number = addressDto.Number,
                Floor = addressDto.Floor,
                PostalCode = addressDto.PostalCode,
                City = addressDto.City,
                IsDefault = addressDto.IsDefault
            };

            await _context.Addresses.AddAsync(newAddress);
            await _context.SaveChangesAsync();

            // Mapear a entidade criada para o DTO de resposta
            var addressResponseDto = new AddressDto { /* ... preencher os campos ... */ };
            // (O mapeamento completo foi omitido por brevidade, mas deve ser feito como no GET)

            return CreatedAtAction(nameof(GetUserAddresses), new { id = newAddress.Id }, addressResponseDto);
        }

        // PUT: api/addresses/{id} - Atualizar uma morada existente
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAddress(int id, [FromBody] CreateOrUpdateAddressDto addressDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var addressToUpdate = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);
            
            if (addressToUpdate == null)
            {
                return NotFound("Morada não encontrada ou não pertence ao utilizador.");
            }

            // Lógica para garantir que só existe uma morada padrão
            if (addressDto.IsDefault)
            {
                var currentDefault = await _context.Addresses
                    .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault && a.Id != id);
                if (currentDefault != null)
                {
                    currentDefault.IsDefault = false;
                }
            }

            // Atualizar os campos
            addressToUpdate.Alias = addressDto.Alias;
            addressToUpdate.Street = addressDto.Street;
            addressToUpdate.Number = addressDto.Number;
            addressToUpdate.Floor = addressDto.Floor;
            addressToUpdate.PostalCode = addressDto.PostalCode;
            addressToUpdate.City = addressDto.City;
            addressToUpdate.IsDefault = addressDto.IsDefault;

            await _context.SaveChangesAsync();

            return NoContent(); // Resposta 204 No Content indica sucesso sem corpo de resposta
        }


        // DELETE: api/addresses/{id} - Apagar uma morada
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var addressToDelete = await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == id && a.UserId == userId);

            if (addressToDelete == null)
            {
                return NotFound("Morada não encontrada ou não pertence ao utilizador.");
            }

            _context.Addresses.Remove(addressToDelete);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}