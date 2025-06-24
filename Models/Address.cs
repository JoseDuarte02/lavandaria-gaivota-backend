// Models/Address.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LavandariaGaivotaAPI.Models
{
    public class Address
    {
        public int Id { get; set; }

        [Required]
        public string? UserId { get; set; } // Chave estrangeira para o utilizador

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; } // Propriedade de navegação

        [Required]
        [StringLength(100)]
        public string? Alias { get; set; } // Ex: "Casa", "Trabalho"

        [Required]
        [StringLength(200)]
        public string? Street { get; set; } // Rua

        [Required]
        [StringLength(20)]
        public string? Number { get; set; } // Número da porta

        [StringLength(50)]
        public string? Floor { get; set; } // Andar / Fração (opcional)

        [Required]
        [StringLength(10)]
        public string? PostalCode { get; set; } // Código Postal

        [Required]
        [StringLength(100)]
        public string? City { get; set; } // Localidade

        public bool IsDefault { get; set; } = false; // Para marcar como morada principal
    }
}