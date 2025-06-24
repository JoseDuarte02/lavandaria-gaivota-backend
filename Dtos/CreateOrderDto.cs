// Dtos/CreateOrderDto.cs
using System.ComponentModel.DataAnnotations;

namespace LavandariaGaivotaAPI.Dtos
{
    // Este DTO representa um único item que o utilizador adiciona ao seu pedido
    public class CreateOrderItemDto
    {
        [Required]
        public string? ServiceDescription { get; set; } // Ex: "Lavar e Secar - Camisa"

        [Required]
        [Range(1, 100)]
        public int Quantity { get; set; }

        [Required]
        public decimal UnitPrice { get; set; }
    }

    // Este DTO representa o pedido completo enviado pelo frontend
    public class CreateOrderDto
    {
        [Required]
        [MinLength(1, ErrorMessage = "O pedido deve ter pelo menos um item.")]
        public List<CreateOrderItemDto> OrderItems { get; set; } = new List<CreateOrderItemDto>();

        // Adicionamos o ID da morada selecionada
        [Required(ErrorMessage = "É obrigatório selecionar uma morada.")]
        public int AddressId { get; set; }

        public string? Notes { get; set; }
    }
}