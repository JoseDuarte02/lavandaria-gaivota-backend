// Dtos/ResetPasswordDto.cs
using System.ComponentModel.DataAnnotations;

namespace LavandariaGaivotaAPI.Dtos
{
    public class ResetPasswordDto
    {
        [Required]
        public string? Token { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A password deve ter no mínimo 6 caracteres")]
        public string? NewPassword { get; set; }
    }
}