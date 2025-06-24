// Dtos/AddressDto.cs
namespace LavandariaGaivotaAPI.Dtos
{
    public class AddressDto
    {
        public int Id { get; set; }
        public string? Alias { get; set; } // Ex: "Casa", "Trabalho"
        public string? Street { get; set; }
        public string? Number { get; set; }
        public string? Floor { get; set; }
        public string? PostalCode { get; set; }
        public string? City { get; set; }
        public bool IsDefault { get; set; }
    }
}