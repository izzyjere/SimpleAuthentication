using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleAuthentication
{
    public interface ITokenService
    {
        Task<Result<AccessToken>> GetAccessToken(TokenRequest tokenRequest);
    }
}
