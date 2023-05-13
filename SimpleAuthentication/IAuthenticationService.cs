namespace SimpleAuthentication
{
    public interface IAuthenticationService
    {
        Task<Result> ConfirmEmailAsync(string userId, string code);
        Task<Result<string>> ForgotPasswordAsync(string email);
        Task<LoginResult> LoginAsync(LoginRequest request);
        Task<Result> ResetPasswordAsync(string email, string code, string password);
    }
}