using Microsoft.AspNetCore.Identity;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAuthentication
{
    public class Role  : IdentityRole
    {
        public Role() :base() {}
        public string Description { get; set; }
        public Role(string name, string description) : base(name){ 
          Description = description;
        }
    }
}
