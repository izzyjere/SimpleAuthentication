using System.ComponentModel.DataAnnotations;

namespace SimpleAuthentication
{
    public class RegisterModel
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        [MinLength(8)]
        public string Password { get; set; }
        [EmailAddress]
        [Required]
        public string Email { get; set; }
        [Required]
        [Compare(nameof(Password))]
        public string ConfirmPassword { get; set; }
        public string? Phone { get; set; }
        public string? FirstName { get; set; }
        public string? MiddleName { get; set; }
        public string? LastName { get; set; }
        public string? AvatarUrl { get; set; }
        /// <summary>
        /// Default false
        /// </summary>
        public bool IncludeProfile { get; set; }
        /// <summary>
        /// Default true
        /// </summary>
        public bool AutoConfirm { get; set; } = true;
         /// <summary>
         /// If one doesn't exist we'll attempt to create it.
         /// </summary>
        public string? Role { get; set; }
    }
}
