using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace SimpleAuthentication
{
    internal class RoleService : IRoleService
    {
        readonly RoleManager<Role> roleManager;

        public RoleService(RoleManager<Role> roleManager)
        {
            this.roleManager=roleManager;
        }
        public async Task<Result> CreateAsync(string name, string description)
        {
            var result = await roleManager.CreateAsync(new Role(name, description));
            return result.Succeeded ? Result.Success("Role created successfully.") : Result.Failure(result?.Errors?.FirstOrDefault()?.Description??"Failed to create role.");
        }
        public async Task<Result> CreateAsync(Role role)
        {
            var result = await roleManager.CreateAsync(role);
            return result.Succeeded ? Result.Success("Role created successfully.") : Result.Failure(result?.Errors?.FirstOrDefault()?.Description??"Failed to create role.");
        }
        public async Task<Result> DeleteAsync(string role)
        {
            var record = await roleManager.FindByNameAsync(role);
            if (record == null)
            {
                return Result.Failure("Role does not exist.");
            }
            var result = await roleManager.DeleteAsync(record);
            return result.Succeeded ? Result.Success("Role deleted successfully.") : Result.Failure(result.Errors.First().Description);
        }
        public async Task<Role> GetRoleByNameAsync(string name)
        {
            return await roleManager.FindByNameAsync(name);
        }
        public async Task<Role> GetRoleByIdAsync(string id)
        {
            return await roleManager.FindByIdAsync(id);
        }
        public async Task<IEnumerable<Role>> GetAllRolesAsync()
        {
            return await roleManager.Roles.ToListAsync();
        }
        public async Task<Result> UpdateAsync(Role role)
        {
            var result = await roleManager.UpdateAsync(role);
            return result.Succeeded ? Result.Success("Role updated successfully.") : Result.Failure(result.Errors.First().Description);
        }
    }
}
