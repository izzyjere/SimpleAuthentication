using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

using Newtonsoft.Json;

using System.Net;
using System.Security.Claims;
using System.Text;

namespace SimpleAuthentication
{
    public static class Extensions
    {
        public static IServiceCollection AddSimpleAuthentication(this IServiceCollection services, Action<DbContextOptionsBuilder> userStoreOptions, Action<IdentityOptions>? identityOptions = null)
        {
            services.AddSimpleAuthenticationIdentity(userStoreOptions, identityOptions);   
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
        private static IServiceCollection AddSimpleAuthenticationIdentity(this IServiceCollection services, Action<DbContextOptionsBuilder> userStoreOptions, Action<IdentityOptions>? identityOptions = null)
        {
            services.AddHttpContextAccessor();
            services.AddAuthentication();
            services.AddDbContext<IdentityDatabaseContext>(userStoreOptions, ServiceLifetime.Transient);
            services.AddIdentity<User, Role>(options =>
            {
                if (identityOptions != null)
                {
                    identityOptions(options);
                }
                else
                {
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
            services.AddScoped<IUserClaimsPrincipalFactory<User>, SimpleClaimsPrincipalFactory>();
            services.AddScoped<ISeeder, DatabaseSeeder>();
            return services;
        }
        public static IServiceCollection AddSimpleAuthenticationJwt(this IServiceCollection services, SimpleJwtConfig simpleJwtConfig,Action<DbContextOptionsBuilder> userStoreOptions, Action<IdentityOptions>? identityOptions = null)
        {
            if (simpleJwtConfig == null)
            {
                throw new ArgumentNullException(nameof(simpleJwtConfig.Secret));
            }
            var key = Encoding.ASCII.GetBytes(simpleJwtConfig.Secret);
            services.AddSimpleAuthenticationIdentity(userStoreOptions, identityOptions);
            services.AddScoped<ITokenService, TokenService>();
            services
                .AddAuthentication(authentication =>
                {
                    authentication.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    authentication.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(bearer =>
                {
                    bearer.RequireHttpsMetadata = false;
                    bearer.SaveToken = true;
                    bearer.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateAudience = false,
                        ValidateIssuer = false,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        RoleClaimType = ClaimTypes.Role,
                        ClockSkew = TimeSpan.Zero
                    };
                    bearer.Events = new JwtBearerEvents
                    {
                        OnAuthenticationFailed = c =>
                        {
                            Console.WriteLine(c.Exception.Message+c.Exception.StackTrace);
                            if (c.Exception is SecurityTokenExpiredException)
                            {
                                c.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                c.Response.ContentType = "application/json";
                                var result = JsonConvert.SerializeObject(Result.Failure("Your Token Has Expired"));
                                return c.Response.WriteAsync(result);
                            }
                            else
                            {
                                c.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                c.Response.ContentType = "application/json";
                                var result = JsonConvert.SerializeObject(Result.Failure("An unhandled server error has occurred."));
                                return c.Response.WriteAsync(result);
                            }
                        },
                        OnChallenge = context =>
                        {
                            context.HandleResponse();
                            if (!context.Response.HasStarted)
                            {
                                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                context.Response.ContentType = "application/json";
                                var result = JsonConvert.SerializeObject(Result.Failure("You are not Authorized."));
                                return context.Response.WriteAsync(result);
                            }

                            return Task.CompletedTask;
                        },
                        OnForbidden = context =>
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            context.Response.ContentType = "application/json";
                            var result = JsonConvert.SerializeObject(Result.Failure("You are not authorized to access this resource."));
                            return context.Response.WriteAsync(result);
                        },
                    };
                });
            services.AddAuthorization();
            return services;
        }
    }

}
