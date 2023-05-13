using Microsoft.AspNetCore.Identity;

namespace SimpleAuthentication
{
    public class User : IdentityUser
    {
        public virtual Profile? Profile { get; set; }
        public bool IsActive { get; set; }
    }
}
