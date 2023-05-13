namespace SimpleAuthentication
{
    public class Profile
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? MiddleName { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime Created { get; set; }
        public Profile()
        {
            Created = DateTime.Now;
        }
    }
}
