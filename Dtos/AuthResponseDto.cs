// Dtos/AuthResponseDto.cs
namespace LavandariaGaivotaAPI.Dtos
{
    public class AuthResponseDto
    {
        public string? Token { get; set; }
        public DateTime Expiration { get; set; }
        public string? UserId { get; set; }
        public string? Email { get; set; }
        public string? FullName { get; set; } // Opcional, mas útil para o frontend
        // Adicione outros dados do utilizador que queira retornar
    }
}