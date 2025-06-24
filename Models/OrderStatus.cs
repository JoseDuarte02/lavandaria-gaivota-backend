// Models/OrderStatus.cs
namespace LavandariaGaivotaAPI.Models
{
    public enum OrderStatus
    {
        Pendente,       // Pending
        Recolhido,      // Collected
        EmLavagem,      // Washing
        ProntoParaEntrega, // Ready for Delivery
        Entregue,       // Delivered
        Cancelado       // Cancelled
    }
}