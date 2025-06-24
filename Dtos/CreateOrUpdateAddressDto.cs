// Dtos/CreateOrUpdateAddressDto.cs
using System.ComponentModel.DataAnnotations;

namespace LavandariaGaivotaAPI.Dtos
{
    public class CreateOrUpdateAddressDto
    {
        [Required(ErrorMessage = "O alias da morada é obrigatório (ex: Casa, Trabalho)")]
        [StringLength(100)]
        public string? Alias { get; set; }

        [Required(ErrorMessage = "A rua é obrigatória")]
        [StringLength(200)]
        public string? Street { get; set; }

        [Required(ErrorMessage = "O número da porta é obrigatório")]
        [StringLength(20)]
        public string? Number { get; set; }

        [StringLength(50)]
        public string? Floor { get; set; } // Opcional

        [Required(ErrorMessage = "O código postal é obrigatório")]
        [RegularExpression(@"^\d{4}-\d{3}$", ErrorMessage = "O código postal deve ter o formato XXXX-XXX.")]
        [StringLength(10)]
        public string? PostalCode { get; set; }

        [Required(ErrorMessage = "A localidade é obrigatória")]
        [StringLength(100)]
        public string? City { get; set; }

        public bool IsDefault { get; set; } = false;
    }
}