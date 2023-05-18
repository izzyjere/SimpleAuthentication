using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAuthentication
{
    internal class SecretConfigService
    {
        public SimpleJwtConfig SimpleJwtConfig { get; }
        public SecretConfigService(SimpleJwtConfig simpleJwtConfig)
        {
               SimpleJwtConfig = simpleJwtConfig;
        }
    }
}
