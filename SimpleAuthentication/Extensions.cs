using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authorization;

namespace SimpleAuthentication
{
    public static class Extensions
    {
        public static IServiceCollection AddSimpleAuthentication(this IServiceCollection services, Action<DbContextOptionsBuilder> userStoreOptions, Action<IdentityOptions>? identityOptions = null)
        {
            services.AddHttpContextAccessor();
            services.AddAuthentication();
            services.AddDbContext<IdentityDatabaseContext>(userStoreOptions,ServiceLifetime.Transient);
            services.AddIdentity<User, Role>(options =>
            {
                if(identityOptions != null)
                {
                    identityOptions(options);
                }
                else {
                    options.Password.RequiredLength = 8;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.User.RequireUniqueEmail = true;
                    options.SignIn.RequireConfirmedEmail = false;
                    options.SignIn.RequireConfirmedAccount = false;
                    options.SignIn.RequireConfirmedPhoneNumber = false;
                }
                
            })
              .AddDefaultTokenProviders()
              .AddEntityFrameworkStores<IdentityDatabaseContext>(); 
            services.AddScoped<ISeeder,DatabaseSeeder>();
            services.AddScoped<IAuthenticationService, AuthenticationService>()
                    .AddScoped<IUserService, UserService>()
                    .AddScoped<IRoleService, RoleService>();
            return services;
        }
        public static IApplicationBuilder UseSimpleAuthentication(this IApplicationBuilder app)
        {
            app.UseMiddleware<AuthenticationMiddleware>();
            app.SeedSystemUser();
            return app;
        }
        internal static IApplicationBuilder SeedSystemUser(this IApplicationBuilder app)
        {
            var scope = app.ApplicationServices.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<ISeeder>();
            if(seeder != null)
            {
                seeder.Seed();
            }
            else
            {
                throw new ArgumentException("Simple authentication hasn't been configured properly.");
            }
            return app;
        }
    }
}
