using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

using System.Collections.Concurrent;

namespace SimpleAuthentication
{
    internal class AuthenticationMiddleware
    {
        readonly RequestDelegate next;
        readonly ILogger<AuthenticationMiddleware> logger;
        static IDictionary<Guid, LoginRequest> Logins { get; set; }
               = new ConcurrentDictionary<Guid, LoginRequest>();
        public static Guid AnnounceLogin(LoginRequest request)
        {
            request.LoginStarted = DateTime.Now;
            var key = Guid.NewGuid();
            Logins.TryAdd(key, request);
            return key;
        }
        public static LoginRequest GetLoginInProgress(Guid key)
        {
            if (Logins.ContainsKey(key))
            {
                return Logins[key];
            }
            return new LoginRequest();
        }
        public static LoginRequest GetLoginInProgress(string key)
        {
            return GetLoginInProgress(Guid.Parse(key));
        }
        public AuthenticationMiddleware(RequestDelegate requestDelegate, ILogger<AuthenticationMiddleware> logger)
        {
            next = requestDelegate;
            this.logger = logger;
        }

        public async Task Invoke(HttpContext context, SignInManager<User> signInManager)
        {
           
            if (context.Request.Path == "/login" && context.Request.Query.ContainsKey("key"))
            {
                var key = Guid.Parse(context.Request.Query["key"]);
                var AuthenticationRequest = Logins[key];
                var result = await signInManager.PasswordSignInAsync(AuthenticationRequest.UserName, AuthenticationRequest.Password, AuthenticationRequest.RememberMe, false);
                if (result.Succeeded)
                {
                    Logins.Remove(key);
                    logger.LogInformation("User {0} logged in successfully.",AuthenticationRequest.UserName);
                    context.Response.Redirect(AuthenticationRequest.ReturnUrl);
                    return;
                }
                else if (result.RequiresTwoFactor)
                {
                    context.Response.Redirect("/loginWith2fa/" + key);
                    return;
                }
                else if (result.IsLockedOut)
                {
                    return;
                }
                else
                {

                    await next.Invoke(context);
                    return;
                }
            }
            else if (context.Request.Path.StartsWithSegments("/loginWith2fa"))
            {
                var key = Guid.Parse(context.Request.Path.Value.Split('/').Last());
                var AuthenticationRequest = Logins[key];
                if (string.IsNullOrEmpty(AuthenticationRequest.TwoFactorCode))
                {
                    //user login 2fa for the first time
                    
                }
                else
                {
                    var result = await signInManager.TwoFactorAuthenticatorSignInAsync(AuthenticationRequest.TwoFactorCode, AuthenticationRequest.RememberMe, AuthenticationRequest.RemberMachine);
                    if (result.Succeeded)
                    {
                        Logins.Remove(key);
                        context.Response.Redirect(AuthenticationRequest.ReturnUrl);
                        return;
                    }
                    else if (result.IsLockedOut)
                    {
                        return;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            else if (context.Request.Path.StartsWithSegments("/logout"))
            {
                await signInManager.SignOutAsync();
                context.Response.Redirect("/");
                return;
            }
            //We get here? then something went wrong
            //continue the http middleware chain

            await next.Invoke(context);
        }
    }
}
