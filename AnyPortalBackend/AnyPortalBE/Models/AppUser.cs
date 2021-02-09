using Microsoft.AspNetCore.Identity;

namespace AnyPortalBE.Models
{
    public class AppUser: IdentityUser
    {
        public string Name { get; set; }
    }
}
