namespace SimpleAuthentication
{
    public class UserProxy
    {
        public string Id { get;  }
        public bool IsActive { get;}
        public Profile? Profile { get; }

        public UserProxy(string id, string userName, string email,bool isActive, string? phone, Profile? profile)
        {
            Id=id;
            UserName=userName;
            Email=email;
            IsActive=isActive;
            Phone=phone;
            Profile=profile;
        }
        List<string> _roles = new();
        internal void AddRoles(IEnumerable<string>? roles)
        {
            if(roles is null)
            {
                return;
            }
           _roles.AddRange(roles);
        }
        public string UserName { get;  }
        public string Email { get;  }
        public string? Phone { get;  }
        public IEnumerable<string> Roles { get=>_roles;  } 
    }
}
