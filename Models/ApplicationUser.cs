// Models/ApplicationUser.cs
using Microsoft.AspNetCore.Identity;

namespace LavandariaGaivotaAPI.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
    
    }
}