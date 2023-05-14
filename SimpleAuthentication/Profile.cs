using System.ComponentModel.DataAnnotations;

namespace SimpleAuthentication
{
    public class Profile
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        public string? MiddleName { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime Created { get; set; }
        public Profile()
        {
            Created = DateTime.Now;
        }
    }
}
