using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SimpleAuthentication
{
    internal class TokenService : ITokenService
    {
        readonly UserManager<User> _userManager;
        readonly SignInManager<User> _signInManager;
        readonly RoleManager<Role> _roleManager;
        readonly ILogger<TokenService> _logger;
        public async Task<Result<AccessToken>> GetAccessToken(TokenRequest tokenRequest)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(tokenRequest.UserName);
                if (user == null)
                {
                    return Result<AccessToken>.Failure("User Not Found.");
                }
                if (!user.IsActive)
                {
                    return Result<AccessToken>.Failure("User Not Active. Please contact the administrator.");
                }
                if (!user.EmailConfirmed)
                {
                    return Result<AccessToken>.Failure("Email not confirmed");
                }
                if (await _signInManager.CanSignInAsync(user))
                {
                    if (await _userManager.IsLockedOutAsync(user))
                    {
                        return Result<AccessToken>.Failure("Too many failed attempts. Try after sometime.");
                    }
                    var result = await _signInManager.CheckPasswordSignInAsync(user, tokenRequest.Password, true);
                    if (result.Succeeded)
                    {
                        
                        user.RefreshToken = GenerateRefreshToken();
                        user.RefreshTokenExpiryTime = DateTime.Now.AddDays(7);
                        await _userManager.UpdateAsync(user);
                        var token = await GenerateJwtAsync(user);
                        var response = new AccessToken(token, DateTime.Now.AddMinutes(60), user.RefreshToken, user.RefreshTokenExpiryTime);
                        return Result<AccessToken>.Success(response);
                    }
                    else
                    {
                        return Result<AccessToken>.Failure("Incorrect Credentials.");
                    }
                }
                else
                {
                    return  Result<AccessToken>.Failure("Your has been locked. Please contact your admin");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message + e.StackTrace);
                return Result<AccessToken>.Failure("An Error Occured. Please Try Again");
            }


        }
        public async Task<Result<AccessToken>> GetRefreshTokenAsync(RefreshTokenRequest model)
        {
            if (model is null)
            {
                return  Result<AccessToken>.Failure("Invalid Client Token");
            }
            var claimsPrincipal = GetPrincipalFromExpiredToken(model.Token);
            var userEmail = claimsPrincipal.GetEmail();
            var user = await _userManager.FindByIdAsync(userEmail);
            if (user == null)
            {
                return Result<AccessToken>.Failure("User not found.");
            }
            if (user.RefreshToken != model.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return  Result<AccessToken>.Failure("Invalid Client Token.");
            }
            var token = GenerateEncryptedToken(GetSigningCredentials(), await GetClaimsAsync(user));
            user.RefreshToken = GenerateRefreshToken();
            await _userManager.UpdateAsync(user);
            var response = new AccessToken(token,DateTime.Now.AddMinutes(60),user.RefreshToken,user.RefreshTokenExpiryTime);
            return Result<AccessToken>.Success(response);
        }
        private async Task<string> GenerateJwtAsync(User user)
        {
            var token = GenerateEncryptedToken(GetSigningCredentials(), await GetClaimsAsync(user));
            return token;
        }
        private async Task<IEnumerable<Claim>> GetClaimsAsync(User user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();
            var permissionClaims = new List<Claim>();
            foreach (var role in roles)
            {
                roleClaims.Add(new Claim(ClaimTypes.Role, role));
                var thisRole = await _roleManager.FindByNameAsync(role);
                var allPermissionsForThisRoles = await _roleManager.GetClaimsAsync(thisRole);
                permissionClaims.AddRange(allPermissionsForThisRoles);
            }

            var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Email, user.Email),
        new(ClaimTypes.MobilePhone, user.PhoneNumber ?? string.Empty)
    }
            .Union(userClaims)
            .Union(roleClaims)
            .Union(permissionClaims);

            return claims;
        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private SigningCredentials GetSigningCredentials()
        {
            var secret = Encoding.UTF8.GetBytes("");
            return new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256);
        }
        private string GenerateEncryptedToken(SigningCredentials signingCredentials, IEnumerable<Claim> claims)
        {
            var token = new JwtSecurityToken(
               claims: claims,
               expires: DateTime.UtcNow.AddMinutes(60),
               signingCredentials: signingCredentials);
            var tokenHandler = new JwtSecurityTokenHandler();
            var encryptedToken = tokenHandler.WriteToken(token);
            return encryptedToken;
        }
        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("")),
                ValidateIssuer = false,
                ValidateAudience = false,
                RoleClaimType = ClaimTypes.Role,
                ClockSkew = TimeSpan.Zero
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }


    }
}
