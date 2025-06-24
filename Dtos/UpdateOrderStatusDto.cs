// Dtos/UpdateOrderStatusDto.cs
using System.ComponentModel.DataAnnotations;
using LavandariaGaivotaAPI.Models;

namespace LavandariaGaivotaAPI.Dtos
{
    public class UpdateOrderStatusDto
    {
        [Required]
        // Garante que o valor recebido é um nome válido do enum OrderStatus
        [EnumDataType(typeof(OrderStatus))]
        public string? Status { get; set; }
    }
}