using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System.Text;

namespace SimpleAuthentication
{
    internal class UserService : IUserService
    {
        readonly UserManager<User> _userManager;
        readonly RoleManager<Role> _roleManager;
        readonly ILogger<UserService> _logger;
        readonly IHttpContextAccessor _httpContextAccessor;
        public UserService(UserManager<User> userManager,RoleManager<Role> roleManager, IHttpContextAccessor httpContextAccessor, ILogger<UserService> logger)
        {

            _userManager=userManager;
            _roleManager = roleManager;
            _httpContextAccessor=httpContextAccessor;
            _logger=logger;
        }
        async Task<UserProxy> ToProxy(User user)
        {
            var proxy = new UserProxy(user.Id, user.UserName, user.Email,user.IsActive, user.PhoneNumber, user.Profile,user.RefreshToken,user.RefreshTokenExpiryTime);
            proxy.AddRoles(await _userManager.GetRolesAsync(user));
            return proxy;
        }
        public async Task<UserProxy?> GetByIdAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return null;
            }
            return await ToProxy(user);
        }
        public async Task<UserProxy?> GetByUserNameAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return null;
            }
            return await ToProxy(user);
        }
        public async Task<IEnumerable<UserProxy>> GetAllAsync()
        {
            var users = new List<UserProxy>();
            await _userManager.Users.ForEachAsync(async (user) =>
            {
                users.Add(await ToProxy(user));
            });
            return users;
        }
        public async Task<UserProxy?> GetCurrentUserAsync()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.Name == null)
            {
                return await GetByUserNameAsync("system");
            }
            return await GetByUserNameAsync(httpContext.User.Identity.Name);
        }
        public async Task<IEnumerable<UserProxy>> GetAllInRoleAsync(string roleName)
        {
            var usersInRole = new List<UserProxy>();
            var users = await _userManager.GetUsersInRoleAsync(roleName);
            foreach (var user in users)
            {
                usersInRole.Add(await ToProxy(user));
            }
            return usersInRole;
        }
        public async Task<Result> AddUserToRoleAsync(string roleName, string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return Result.Failure("Error: User not found.");
            }              
            var result = await _userManager.AddToRoleAsync(user, roleName);
            return result.Succeeded ? Result.Success("User successfully added to role.") : Result.Failure(result.Errors.First().Description);

        }
        public async Task<Result> CreateAsync(string userName, string email, string password, string? phoneNumber, bool autoConfirm = true)
        {
            var userWithSameName = await _userManager.FindByNameAsync(userName);
            if (userWithSameName != null)
            {
                _logger.LogWarning("User creation failed : Duplicate username.");
                return Result.Failure($"Username {userName} is already taken.");
            }
            var userWithSameEmail = await _userManager.FindByEmailAsync(email);
            if (userWithSameEmail != null)
            {
                _logger.LogWarning($"User creation failed : Duplicate email.");
                return Result.Failure($"Email {email} is already taken.");
            }
            var user = new User
            {
                UserName = userName,
                Email = email,
                PhoneNumber = phoneNumber,
                EmailConfirmed = autoConfirm,
                IsActive = true
            };
            var result = await _userManager.CreateAsync(user, password);
            if (result.Succeeded)
            {
                _logger.LogInformation("User created with password.");
                return Result.Success("User created successfully.");
            }
            return Result.Failure(result.Errors.First().Description);
        }
        public async Task<Result> UpdateAsync(string id, string email, string phoneNumber)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Result.Failure("User not found.");
            }
            user.Email = email;
            user.PhoneNumber = phoneNumber;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("User details updated.");
                return Result.Success("User updated successfully.");
            }
            return Result.Failure(result.Errors.First().Description);

        }
        public async Task<Result> DeleteAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Result.Failure("User not found.");
            }
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("User deleted.");
                return Result.Success("User deleted successfully.");
            }
            return Result.Failure(result.Errors.First().Description);
        }
        public async Task<Result> UpdateUserProfile(string id, string firstName, string lastName, string? middleName, string? avatarUrl)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return Result.Failure("User not found.");
            }
            user.Profile ??= new();
            user.Profile.FirstName = firstName;
            user.Profile.LastName = lastName;
            user.Profile.MiddleName = middleName;
            user.Profile.AvatarUrl = avatarUrl;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                _logger.LogInformation("User profile updated.");
                return Result.Success("User profile updated.");
            }
            return Result.Failure(result.Errors.First().Description);
        }
        public async Task<UserProxy?> GetByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return null;
            }
            return await ToProxy(user);
        }

        public async Task<Result> CreateAsync(RegisterModel registerModel)
        {
            var userWithSameName = await _userManager.FindByNameAsync(registerModel.UserName);
            if (userWithSameName != null)
            {
                _logger.LogWarning("User creation failed : Duplicate username.");
                return Result.Failure($"Username {registerModel.UserName} is already taken.");
            }
            var userWithSameEmail = await _userManager.FindByEmailAsync(registerModel.Email);
            if (userWithSameEmail != null)
            {
                _logger.LogWarning($"User creation failed : Duplicate email.");
                return Result.Failure($"Email {registerModel.Email} is already taken.");
            }
            var user = new User
            {
                UserName = registerModel.UserName,
                Email = registerModel.Email,
                PhoneNumber = registerModel.Phone,
                EmailConfirmed = registerModel.AutoConfirm,
                IsActive = true
            };
            if (registerModel.IncludeProfile)
            {
                user.Profile = new Profile
                {
                   FirstName = registerModel.FirstName,
                   LastName = registerModel.LastName,
                   MiddleName = registerModel.MiddleName,
                   AvatarUrl = registerModel.AvatarUrl
                };
            }
            else { }
            var result = await _userManager.CreateAsync(user, registerModel.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("User created with password.");
                if(!string.IsNullOrEmpty(registerModel.Role))
                {
                    var role = await _roleManager.FindByNameAsync(registerModel.Role);
                    if (role == null)
                    {
                        _logger.LogWarning("Role {0} was not found. attempting to create it.", registerModel.Role);
                        await _roleManager.CreateAsync(new Role(registerModel.Role, registerModel.Role));
                    }
                    else { }
                    await AddUserToRoleAsync(registerModel.Role, user.UserName);
                }
                return Result.Success("User created successfully.");
            }
            return Result.Failure(result.Errors.First().Description);
        }
        public async Task<Result<string>> GetEmailConfirmationCodeAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return Result<string>.Failure("Something went wrong.");
            }
            if (user.EmailConfirmed)
            {
                return Result<string>.Failure("User email already confirmed.");
            }
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            return Result<string>.Success(code);    
        }
    }
}
