using Microsoft.AspNetCore.Identity;

namespace SimpleAuthentication
{
    public class Role  : IdentityRole
    {
        public Role() :base() {}
        public string Description { get; set; }
        public Role(string name, string description) : base(name){ 
          Description = description;
        }
    }
}
