namespace SimpleAuthentication
{
    public interface IUserService
    {
        Task<Result> AddUserToRoleAsync(string roleName, string userName);
        Task<Result> CreateAsync(string userName, string email, string password, string? phoneNumber, bool autoConfirm = true);
        Task<Result> CreateAsync(RegisterModel registerModel);
        Task<Result> DeleteAsync(string id);
        Task<IEnumerable<UserProxy>> GetAllAsync();
        Task<IEnumerable<UserProxy>> GetAllInRoleAsync(string roleName);
        Task<UserProxy?> GetByEmailAsync(string email);
        Task<UserProxy?> GetByIdAsync(string id);        
        Task<UserProxy?> GetByUserNameAsync(string userName);
        Task<UserProxy?> GetCurrentUserAsync();
        Task<Result<string>> GetEmailConfirmationCodeAsync(string email);
        Task<Result> UpdateAsync(string id, string email, string phoneNumber);
        Task<Result> UpdateUserProfile(string id, string firstName, string lastName, string? middleName, string? avatarUrl);
    }
}