using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

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
            services.AddScoped<IAuthenticationService, AuthenticationService>();
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
            if (seeder != null)
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
                    options.SignIn.RequireConfirmedEmail = false;
                }

            })
              .AddDefaultTokenProviders()
              .AddEntityFrameworkStores<IdentityDatabaseContext>();
            services.AddScoped<IUserService, UserService>()
                 .AddScoped<IRoleService, RoleService>();
            services.AddScoped<IUserClaimsPrincipalFactory<User>, SimpleClaimsPrincipalFactory>();
            services.AddScoped<ISeeder, DatabaseSeeder>();
            return services;
        }
        private static SimpleJwtConfig GetApplicationSettings(
     this IServiceCollection services,
     IConfiguration configuration)
        {
            var applicationSettingsConfiguration = configuration.GetSection(nameof(SimpleJwtConfig));
            services.Configure<SimpleJwtConfig>(applicationSettingsConfiguration);
            return applicationSettingsConfiguration.Get<SimpleJwtConfig>();
        }

        private static IServiceCollection SimpleJwtConfigure(
          this IServiceCollection services,
          IConfiguration configuration)
        {
            var applicationSettingsConfiguration = configuration.GetSection(nameof(SimpleJwtConfig));
            services.Configure<SimpleJwtConfig>(applicationSettingsConfiguration);
            return services;
        }
        public static WebApplicationBuilder UseSimpleAuthenticationJwt(this WebApplicationBuilder builder, Action<DbContextOptionsBuilder> userStoreOptions, Action<IdentityOptions>? identityOptions = null, OpenApiInfo? openApiInfo = null)
        {
            builder.Services.SimpleJwtConfigure(builder.Configuration);
            var simpleJwtConfig = builder.Services.GetApplicationSettings(builder.Configuration)?? throw new ArgumentNullException(message:"Unable to find SimpleJwtConfig in appsettings.json",paramName:nameof(SimpleJwtConfig));
            var key = Encoding.ASCII.GetBytes(simpleJwtConfig.Secret);
            builder.Services.AddSimpleAuthenticationIdentity(userStoreOptions, identityOptions);
            builder.Services.AddSingleton(sp => new SecretConfigService(simpleJwtConfig));
            builder.Services.AddScoped<ITokenService, TokenService>();
            builder.Services
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
            builder.Services.AddAuthorization();
            builder.Services.RegisterSwagger(openApiInfo);
            var scope = builder.Services.BuildServiceProvider().CreateScope();
            var seeder = scope.ServiceProvider.GetService<ISeeder>();
            seeder?.Seed();
            return builder;
        }
        internal static IServiceCollection RegisterSwagger(this IServiceCollection services, OpenApiInfo? openApiInfo = null)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(async c =>
            {
                //TODO - Lowercase Swagger Documents
                //c.DocumentFilter<LowercaseDocumentFilter>();
                //Refer - https://gist.github.com/rafalkasa/01d5e3b265e5aa075678e0adfd54e23f

                // include all project's xml comments
                var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (!assembly.IsDynamic)
                    {
                        var xmlFile = $"{assembly.GetName().Name}.xml";
                        var xmlPath = Path.Combine(baseDirectory, xmlFile);
                        if (File.Exists(xmlPath))
                        {
                            c.IncludeXmlComments(xmlPath);
                        }
                    }
                }
              
                c.SwaggerDoc("v1", openApiInfo??new OpenApiInfo
                {
                    Version = "v1",
                    Title = "SIMPLE JWT API"
                });    

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "Input your Bearer token in this format - Bearer {your token here} to access this API",
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                    Scheme = "Bearer",
                    Name = "Bearer",
                    In = ParameterLocation.Header,
                }, new List<string>()
            },
            });
            });
            return services;
        }
    }

}
