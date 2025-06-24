// Dtos/ForgotPasswordDto.cs
using System.ComponentModel.DataAnnotations;

namespace LavandariaGaivotaAPI.Dtos
{
    public class ForgotPasswordDto
    {
        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string? Email { get; set; }
    }
}