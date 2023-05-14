using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAuthentication
{
    internal class SimpleClaimsPrincipalFactory : UserClaimsPrincipalFactory<User, Role>
    {
        public SimpleClaimsPrincipalFactory(UserManager<User> userManager, RoleManager<Role> roleManager, IOptions<IdentityOptions> options) : base(userManager, roleManager, options)
        {
        }
        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(User user)
        {
            try
            {
               ;
                var identity = await base.GenerateClaimsAsync(user);
              
                if (user.Profile!=null)
                {
                    identity.AddClaim(new Claim("FullName", user.Profile.FirstName + " " + user.Profile.LastName));
                    identity.AddClaim(new Claim("FirstName", user.Profile.FirstName));
                    identity.AddClaim(new Claim("LastName", user.Profile.LastName));
                    identity.AddClaim(new Claim("Id", user.Id));
                }
                return identity;
            }
            catch (Exception e)
            {
                Console. WriteLine(e.Message+e.StackTrace);
                return default;
            }

        }
    }
}
