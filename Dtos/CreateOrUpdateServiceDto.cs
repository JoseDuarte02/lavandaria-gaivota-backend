// Dtos/CreateOrUpdateServiceDto.cs
using System.ComponentModel.DataAnnotations;

namespace LavandariaGaivotaAPI.Dtos
{
    public class CreateOrUpdateServiceDto
    {
        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(250)]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, 10000.00)]
        public decimal Price { get; set; }

        [Required]
        [StringLength(20)]
        public string? Unit { get; set; }
    }
}