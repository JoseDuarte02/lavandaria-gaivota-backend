// Controllers/ServicesController.cs
using LavandariaGaivotaAPI.Data;
using LavandariaGaivotaAPI.Dtos;
using LavandariaGaivotaAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LavandariaGaivotaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/services - Endpoint PÚBLICO para buscar todos os serviços
        [HttpGet]
        public async Task<IActionResult> GetAllServices()
        {
            var services = await _context.Services
                .Select(s => new ServiceDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    Price = s.Price,
                    Unit = s.Unit
                })
                .ToListAsync();
            
            return Ok(services);
        }

        // --- MÉTODOS DE ADMINISTRAÇÃO ---

        // POST: api/services - Criar um novo serviço (Admin)
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateService([FromBody] CreateOrUpdateServiceDto serviceDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var newService = new Service
            {
                Name = serviceDto.Name,
                Description = serviceDto.Description,
                Price = serviceDto.Price,
                Unit = serviceDto.Unit
            };

            await _context.Services.AddAsync(newService);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllServices), new { id = newService.Id }, newService);
        }

        // PUT: api/services/{id} - Atualizar um serviço (Admin)
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateService(int id, [FromBody] CreateOrUpdateServiceDto serviceDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var serviceToUpdate = await _context.Services.FindAsync(id);
            if (serviceToUpdate == null)
            {
                return NotFound();
            }

            serviceToUpdate.Name = serviceDto.Name;
            serviceToUpdate.Description = serviceDto.Description;
            serviceToUpdate.Price = serviceDto.Price;
            serviceToUpdate.Unit = serviceDto.Unit;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/services/{id} - Apagar um serviço (Admin)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteService(int id)
        {
            var serviceToDelete = await _context.Services.FindAsync(id);
            if (serviceToDelete == null)
            {
                return NotFound();
            }

            _context.Services.Remove(serviceToDelete);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}