// Models/Service.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LavandariaGaivotaAPI.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; } // Ex: "Lavar e Secar", "Engomadoria"

        [StringLength(250)]
        public string? Description { get; set; } // Ex: "Roupa do dia-a-dia", "Camisa social"

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; } // Preço do serviço

        [Required]
        [StringLength(20)]
        public string? Unit { get; set; } // Ex: "Kg", "unid.", "peça"
    }
}