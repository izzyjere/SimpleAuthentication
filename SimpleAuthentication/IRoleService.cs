namespace SimpleAuthentication
{
    public interface IRoleService
    {
        Task<Result> CreateAsync(Role role);
        Task<Result> CreateAsync(string name, string description);
        Task<Result> DeleteAsync(string role);
        Task<IEnumerable<Role>> GetAllRolesAsync();
        Task<Role> GetRoleByIdAsync(string id);
        Task<Role> GetRoleByNameAsync(string name);
        Task<Result> UpdateAsync(Role role);
    }
}