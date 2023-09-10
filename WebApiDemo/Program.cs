using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using SimpleAuthentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(async c =>
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

    c.SwaggerDoc("v1", new OpenApiInfo
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
builder.Services.AddSimpleAuthentication(options =>
{
    options.UseSqlite("Data Source = demo.db", o2 =>
    {
        o2.MigrationsAssembly(typeof(Program).Assembly.FullName);
    });
}, useJwt: true);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseAuthentication();
app.UseAuthorization(); 
//app.UseHttpsRedirection();
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", [Authorize] () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateTime.Now.AddDays(index),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");
//JWT Token EndPoint
app.MapPost("/token", async (ITokenService tokenService, [FromBody] TokenRequest request) =>
{
    return await tokenService.GetAccessToken(request);
});
app.MapPost("/createUser", async (IUserService userService, [FromBody] RegisterModel request) =>
{
    return await userService.CreateAsync(request);
});

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}