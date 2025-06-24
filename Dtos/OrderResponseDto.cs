// Dtos/OrderResponseDto.cs
using LavandariaGaivotaAPI.Models; // Para o OrderStatus

namespace LavandariaGaivotaAPI.Dtos
{
    // DTO para representar um item dentro da resposta do pedido
    public class OrderItemResponseDto
    {
        public int Id { get; set; }
        public string? ServiceDescription { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    // DTO para representar a resposta completa do pedido
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? UserFullName { get; set; } // Enviar o nome em vez do objeto User completo
        public DateTime OrderDate { get; set; }
        public string? Status { get; set; } // Enviar o nome do status como string
        public decimal TotalPrice { get; set; }
        public string? PickupAddress { get; set; }
        public string? Notes { get; set; }
        public List<OrderItemResponseDto> OrderItems { get; set; } = new();
    }
}