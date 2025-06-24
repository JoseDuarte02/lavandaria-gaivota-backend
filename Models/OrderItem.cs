// Models/OrderItem.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LavandariaGaivotaAPI.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; } // Chave estrangeira para o pedido

        [ForeignKey("OrderId")]
        public Order? Order { get; set; } // Propriedade de navegação

        [Required]
        [StringLength(100)]
        public string? ServiceDescription { get; set; } // Ex: "Lavar e Secar - Camisa", "Engomar - Calças"

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
    }
}