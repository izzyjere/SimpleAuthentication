using System.ComponentModel.DataAnnotations;

namespace SimpleAuthentication
{
    public class LoginRequest
    {
        public DateTime LoginStarted { get;  set; }

        [Required]
        public virtual string UserName { get; set; }
        [Required]
        public virtual string Password { get; set; }
        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
        public string? TwoFactorCode { get; set; }
        public bool RemberMachine { get; set; }
     
    }
}