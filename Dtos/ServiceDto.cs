// Dtos/ServiceDto.cs
namespace LavandariaGaivotaAPI.Dtos
{
    public class ServiceDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Unit { get; set; }
    }
}