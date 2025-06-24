// Dtos/RegisterDto.cs
using System.ComponentModel.DataAnnotations;

namespace LavandariaGaivotaAPI.Dtos
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Nome completo é obrigatório")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "O nome deve ter entre 3 e 100 caracteres")]
        public string? FullName { get; set; } // Corresponde ao que o frontend espera e ao que pode querer guardar

        [Required(ErrorMessage = "Email é obrigatório")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password é obrigatória")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A password deve ter no mínimo 6 caracteres")]
        public string? Password { get; set; }

        // Pode adicionar outros campos que vêm do formulário de registo se necessário
        // public string? Address { get; set; }
        // public string? PhoneNumber { get; set; }
    }
}