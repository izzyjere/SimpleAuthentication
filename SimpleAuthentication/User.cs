using Microsoft.AspNetCore.Identity;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAuthentication
{
    public class User : IdentityUser
    {
        public virtual Profile? Profile { get; set; }
        public bool IsActive { get; set; }
    }
}
