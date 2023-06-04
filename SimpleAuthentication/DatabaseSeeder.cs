using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace SimpleAuthentication
{
    internal class DatabaseSeeder : ISeeder
    {
        readonly UserManager<User> _userManager; 
        readonly IdentityDatabaseContext _context;
        readonly ILogger<DatabaseSeeder> _logger;
        readonly RoleManager<Role> _roleManager;
        public DatabaseSeeder(UserManager<User> userManager,RoleManager<Role> roleManager, IdentityDatabaseContext context,  ILogger<DatabaseSeeder> logger)
        {
            _userManager=userManager;
            _context=context;
            _logger=logger;
            _roleManager = roleManager;
        }

        public async void Seed()
        {
             await _context.Database.EnsureCreatedAsync();
             AddSystemUser();
        }
        private void AddSystemUser()
        {
           Task.Run(async () => {

               var role = await _roleManager.FindByNameAsync("Administrator");
               if(role == null)
               {
                   role = new Role("Administrator", "Default role for the system super user.");
                   await _roleManager.CreateAsync(role);
               }
               else { }
              
               var system = await _userManager.FindByNameAsync("system");
               if(system != null) {
                   return;
               }
               system = new User()
               {
                   UserName = "system",
                   Email = "system@localhost.com",
                   EmailConfirmed = true,
                   IsActive = true,
                   Profile = new Profile
                   {
                      FirstName = "System",
                      LastName  = "Admin",
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
