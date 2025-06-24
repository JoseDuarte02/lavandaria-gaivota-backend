// Models/Order.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LavandariaGaivotaAPI.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string? UserId { get; set; } // Chave estrangeira para o utilizador

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; } // Propriedade de navegação

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        [Required]
        public OrderStatus Status { get; set; } = OrderStatus.Pendente;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalPrice { get; set; }

        public string? PickupAddress { get; set; } // Morada de recolha

        public string? Notes { get; set; } // Notas do utilizador

        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>(); // Um pedido tem uma coleção de itens
    }
}