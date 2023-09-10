using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
        public static IServiceCollection AddSimpleAuthentication(this IServiceCollection services, Action<DbContextOptionsBuilder> userStoreOptions, Action<IdentityOptions>? identityOptions = null, bool useJwt = false)
        {
            services.AddSimpleAuthenticationIdentity(userStoreOptions, identityOptions, useJwt);
            services.AddAuthorization();
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            return services;
        }

        public static IApplicationBuilder UseSimpleAuthentication(this IApplicationBuilder app)
        {
            app.UseMiddleware<AuthenticationMiddleware>();
            var scope = app.ApplicationServices.CreateScope();
            var _context = scope.ServiceProvider.GetRequiredService<IdentityDatabaseContext>();
            _context.Database.EnsureCreated();
            return app;
        }
        private static AuthenticationBuilder AddSimpleAuth(this IServiceCollection services, bool useJwt = false)
        {
            var builder = services.AddAuthentication();                                                
            SimpleJwtConfig? config;
            IConfigurationSection? jwtConfiguration;
            if (useJwt)
            {
                jwtConfiguration = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build().GetSection("SimpleJwtConfig")??throw new Exception("Required section 'SimpleJwtConfig' is missing from appsettings.json");
                builder.Services.Configure<SimpleJwtConfig>(jwtConfiguration);
                config  = jwtConfiguration.Get<SimpleJwtConfig>();
            }
            else
            {
                config = null;
            }
            if (config != null && !string.IsNullOrEmpty(config.Secret))
            {
                builder.Services.AddSingleton(sp => new SecretConfigService(config));
                builder.Services.AddScoped<ITokenService, TokenService>();
                var key = Encoding.ASCII.GetBytes(config.Secret);
                builder.AddJwtBearer(bearer =>
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
            }
            return builder;
        }
        private static IServiceCollection AddSimpleAuthenticationIdentity(this IServiceCollection services, Action<DbContextOptionsBuilder> userStoreOptions, Action<IdentityOptions>? identityOptions = null, bool useJwt = false)
        {
            services.AddHttpContextAccessor();
            services.AddSimpleAuth(useJwt);
            services.AddDbContext<IdentityDatabaseContext>(userStoreOptions, ServiceLifetime.Transient);
            services.AddIdentity<User, Role>(options =>
            {
                if (identityOptions != null)
                {
                    identityOptions(options);
                }
                else
                {
                    options.SignIn.RequireConfirmedEmail = false;
                }

            })
              .AddDefaultTokenProviders()
              .AddEntityFrameworkStores<IdentityDatabaseContext>();
            services.AddScoped<IUserService, UserService>()
                 .AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserClaimsPrincipalFactory<User>, SimpleClaimsPrincipalFactory>();
            return services;
        }
    }

}
