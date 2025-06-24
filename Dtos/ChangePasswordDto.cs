// Dtos/ChangePasswordDto.cs
using System.ComponentModel.DataAnnotations;

namespace LavandariaGaivotaAPI.Dtos
{
    public class ChangePasswordDto
    {
        [Required]
        public string? CurrentPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "A nova password deve ter no mínimo 6 caracteres")]
        public string? NewPassword { get; set; }
    }
}