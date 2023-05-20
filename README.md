# SimpleAuthentication
Currently Supports Blazor Server & ASP.NET Core MVC, Web API JWT. Next Release will include a clientside package to support WASM/MAUI - Blazor Hybrid.
## How to set up
Download Nuget Package
```powershell
Install-Package Codelabs.SimpleAuthentication
```
For Blazor Server Apps You Need A Custom AuthenticationState Provider. Just Copy the class below and add it in your project.
```csharp
 public class RevalidatingIdentityAuthenticationStateProvider<TUser>
        : RevalidatingServerAuthenticationStateProvider where TUser : class
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IdentityOptions _options;

        public RevalidatingIdentityAuthenticationStateProvider(
            ILoggerFactory loggerFactory,
            IServiceScopeFactory scopeFactory,
            IOptions<IdentityOptions> optionsAccessor)
            : base(loggerFactory)
        {
            _scopeFactory = scopeFactory;
            _options = optionsAccessor.Value;
        }

        protected override TimeSpan RevalidationInterval => TimeSpan.FromMinutes(30);

        protected override async Task<bool> ValidateAuthenticationStateAsync(
            AuthenticationState authenticationState, CancellationToken cancellationToken)
        {
            // Get the user manager from a new scope to ensure it fetches fresh data
            var scope = _scopeFactory.CreateScope();
            try
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<TUser>>();
                return await ValidateSecurityStampAsync(userManager, authenticationState.User);
            }
            finally
            {
                if (scope is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync();
                }
                else
                {
                    scope.Dispose();
                }
            }
        }

        private async Task<bool> ValidateSecurityStampAsync(UserManager<TUser> userManager, ClaimsPrincipal principal)
        {
            var user = await userManager.GetUserAsync(principal);
            if (user == null)
            {
                return false;
            }
            else if (!userManager.SupportsUserSecurityStamp)
            {
                return true;
            }
            else
            {
                var principalStamp = principal.FindFirstValue(_options.ClaimsIdentity.SecurityStampClaimType);
                var userStamp = await userManager.GetSecurityStampAsync(user);
                return principalStamp == userStamp;
            }
        }
    }
```
Next Install <code>Microsoft.EntityFrameworkCore.Tools</code> and the Database provider of your choice in the demo
<code>Microsoft.EntityFrameworkCore.SQLite</code> is used. <br/>
Lastly in your <code>Program.cs</code> configure SimpleAuthentication Like below.
```csharp
using Demo;
using Demo.Data;

using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;

using SimpleAuthentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSimpleAuthentication(userStoreOptions =>
{
    userStoreOptions.UseSqlite("Data Source = Identity.db");//Database to use
});
builder.Services.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<User>>();
builder.Services.AddSingleton<WeatherForecastService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();


app.UseRouting(); 

app.UseAuthentication();
app.UseAuthorization();
app.UseSimpleAuthentication();//important

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();

```
The code below shows a demo login page using Razor.
```razor
@page "/login"
@attribute [AllowAnonymous]
@layout LoginLayout
<div class="container center">
    <div class="col-md-6">
        <div class="card">
            <div class="card-header">
                <h4 class="text-primary">Login</h4>
            </div>
            <div class="card-body">
                <EditForm Model="@Model" OnValidSubmit="LoginAsync" id="loginForm">
                    <DataAnnotationsValidator />
                    <div class="form-group mb-4">
                        <label for="username">Username:</label>
                        <InputText @bind-Value="Model.UserName" type="text" class="form-control" id="username" name="username" />
                        <ValidationMessage For="()=>Model.UserName" />
                    </div>
                    <div class="form-group mb-4">
                        <label for="password">Password:</label>
                        <InputText @bind-Value="Model.Password" type="password" class="form-control" id="password" name="password" />
                        <ValidationMessage For="()=>Model.Password" />
                    </div>
                    <button title="Login now." type="submit" class="btn btn-primary">Login</button>
                    <a href="register" title="Don't have an account yet? Register."
                       class="btn btn-success mx-4">Register</a>
                </EditForm>
                @if (failed)
                {
                    <div id="e-message" class="mt-4 pa-2">
                        <div class="alert alert-danger">
                            <div class="d-flex align-items-center">
                                <div>
                                    <i id="e-icon" class="oi oi-warning"></i>
                                </div>
                                <div class="ml-4 mt-3">
                                    <p id="e-message-content">@errorMessage</p>
                                </div>
                            </div>
                        </div>
                    </div>
                }
            </div>
        </div>
    </div>
</div>
@inject NavigationManager navManager;
@code {
    LoginRequest Model = new();
    string errorMessage = string.Empty;
    bool failed;
    async Task LoginAsync()
    {

        Model.ReturnUrl="/fetchdata";
        var authResult = await authenticationService.LoginAsync(Model);
        if(authResult.Succeeded)
        {
            navManager.NavigateTo($"/login?key={authResult.Key}",true);
        }else
        {
            failed = true;
            errorMessage = authResult.Message;
            StateHasChanged();
        }
    }
}

```
## Web APIs JWT
The following Example Shows Easy Set Up From the WebAPI Demo Project <code>Program.cs</code> for Use In ASP.NET Core Web APIs. 
```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using SimpleAuthentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
//Package Already Ships with Swagger.
builder.UseSimpleAuthenticationJwt(options =>
{
    options.UseSqlite("Data Source = demo.db");
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast",[Authorize] () =>
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

app.Run();

internal record WeatherForecast(DateTime Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
```
That's it. For more info see <a href="https://github.com/izzyjere/SimpleAuthentication" target="_blank">Git Hub</a>