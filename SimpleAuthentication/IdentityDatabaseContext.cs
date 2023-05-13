using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SimpleAuthentication
{
    public class IdentityDatabaseContext : IdentityDbContext<User,Role,string>
    {
        public IdentityDatabaseContext(DbContextOptions<IdentityDatabaseContext> options): base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<User>(e =>
            {
                e.ToTable("Users", "Identity");
                e.OwnsOne(u => u.Profile, p =>
                {
                    p.ToTable("UserProfiles", "Identity");                   
                    p.WithOwner();
                });
            });
            builder.Entity<Role>(e =>
            {
                e.ToTable("Roles", "Identity");
            });
            builder.Entity<IdentityUserRole<string>>(e =>
            {
                e.ToTable("UserRoles", "Identity");
                e.HasKey(nameof(IdentityUserRole<string>.UserId),nameof(IdentityUserRole<string>.RoleId));
            });
            builder.Entity<IdentityUserLogin<string>>(e =>
            {
                e.ToTable("UserLogins", "Identity");
                e.HasKey(nameof(IdentityUserLogin<string>.UserId), nameof(IdentityUserLogin<string>.LoginProvider));
            }); 
            builder.Entity<IdentityUserToken<string>>(e =>
            {
                e.ToTable("UserTokens","Identity");
                e.HasKey(nameof(IdentityUserToken<string>.UserId), nameof(IdentityUserToken<string>.LoginProvider));
            });
            
        }
    }
}
