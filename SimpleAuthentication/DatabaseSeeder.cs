using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace SimpleAuthentication
{
    internal class DatabaseSeeder : ISeeder
    {
        readonly UserManager<User> _userManager; 
        readonly IdentityDatabaseContext _context;
        readonly RoleManager<Role> _roleManager;
        readonly ILogger<DatabaseSeeder> _logger;
        public DatabaseSeeder(UserManager<User> userManager, IdentityDatabaseContext context, RoleManager<Role> roleManager, ILogger<DatabaseSeeder> logger)
        {
            _userManager=userManager;
            _context=context;
            _roleManager=roleManager;
            _logger=logger;
        }

        public async void Seed()
        {
            await _context.Database.EnsureCreatedAsync();
             AddSystemUser();
        }
        private void AddSystemUser()
        {
           Task.Run(async () => {
               var role = await _roleManager.FindByNameAsync("System");
               if (role == null)
               {
                   role = new Role("System", "Default role for the system user.");
                   await _roleManager.CreateAsync(role);
               }
               else { }
               var system = await _userManager.FindByNameAsync("System");
               if(system != null) {
                   return;
               }
               system = new User()
               {
                   UserName = "System",
                   Email = "system@localhost.com",
                   EmailConfirmed = true,
                   IsActive = true,
                   Profile = new Profile
                   {
                      FirstName = "Bob",
                      LastName  = "System",
                   }
               };
               var result = await _userManager.CreateAsync(system, "password"); ;
               if(result.Succeeded)
               {
                   await _userManager.AddToRoleAsync(system, role.Name);
                   _logger.LogInformation("Seeded default system user.");
               }                         
           
           }).GetAwaiter().GetResult();
        }
    }
}
