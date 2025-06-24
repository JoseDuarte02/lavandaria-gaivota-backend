// Dtos/LoginDto.cs
using System.ComponentModel.DataAnnotations;

namespace LavandariaGaivotaAPI.Dtos
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password é obrigatória")]
        public string? Password { get; set; }
    }
}