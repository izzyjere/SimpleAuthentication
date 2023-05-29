using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace SimpleAuthentication
{
    internal class AuthenticationService : IAuthenticationService
    {
        readonly SignInManager<User> signInManager;
        readonly UserManager<User> userManager;
        public AuthenticationService(SignInManager<User> signInManager, UserManager<User> userManager)
        {
            this.signInManager=signInManager;
            this.userManager=userManager;             
        }


        public async Task<LoginResult> LoginAsync(LoginRequest request)
        {
            User? user;
            if (MailAddress.TryCreate(request.UserName, out var email))
            {
                 user = await userManager.FindByEmailAsync(email.Address);
            }
            else
            {
                user = await userManager.FindByNameAsync(request.UserName);
            }
             
            if (user == null)
            {
                return LoginResult.Failure("User Not Found.");
            }
            if (!user.IsActive)
            {
                return LoginResult.Failure("User Not Active. Please contact the administrator.");
            }
            if (!user.EmailConfirmed)
            {
                return LoginResult.Failure("Email not confirmed");
            }
            if (await signInManager.CanSignInAsync(user))
            {
                var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, true);
                if (result.Succeeded)
                {
                    var key = AuthenticationMiddleware.AnnounceLogin(request);
                    user.RefreshToken = GenerateRefreshToken();
                    user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
                    await userManager.UpdateAsync(user);
                    return LoginResult.Success(key, $"Logged in as {user.UserName}.");
                }
                else
                {
                    return LoginResult.Failure("Incorrect Credentials.");
                }
            }
            else
            {
                return LoginResult.Failure("Your Account Is Locked Too many attempts. Try again after a few moments.");
            }
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
        public async Task<Result<string>> ForgotPasswordAsync(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null || !await userManager.IsEmailConfirmedAsync(user))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return Result<string>.Failure("An Error has occurred!");
            }
            // For more information on how to enable account confirmation and password reset please
            // visit https://go.microsoft.com/fwlink/?LinkID=532713
            var code = await userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            return Result<string>.Success(code);
        }
        public async Task<Result> ResetPasswordAsync(string email, string code, string password)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return Result.Failure("An Error has occured!");
            }
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await userManager.ResetPasswordAsync(user, code,password);
            if (result.Succeeded)
            {
                return Result.Success("Password Reset Successful!");
            }
            else
            {
                return Result.Failure(result.Errors.FirstOrDefault()?.Description??"An error has occured.");
            }
        }
        public async Task<Result> ConfirmEmailAsync(string userId, string code)
        {
            var user = await userManager.FindByIdAsync(userId);
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                return Result.Success(string.Format("Account Confirmed for {0}", user.Email));
            }
            else
            {
               return Result.Failure( string.Format("An error occurred while confirming {0}", user.Email));
            }
        }
    }
}
