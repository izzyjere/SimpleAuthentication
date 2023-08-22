using System.ComponentModel.DataAnnotations;

namespace SimpleAuthentication
{
    public class RegisterModel
    {
        [Required]
        public virtual string UserName { get; set; }
        [Required]
        [MinLength(8)]
        public virtual string Password { get; set; }
        [EmailAddress]
        [Required]
        public virtual string Email { get; set; }
        [Required]
        [Compare(nameof(Password))]
        public virtual string ConfirmPassword { get; set; }
        public virtual string? Phone { get; set; }
        public virtual string? FirstName { get; set; }
        public virtual string? MiddleName { get; set; }
        public virtual string? LastName { get; set; }
        public virtual string? AvatarUrl { get; set; }
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
        public virtual string? Role { get; set; }
    }
}
