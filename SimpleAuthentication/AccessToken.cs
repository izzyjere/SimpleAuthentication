namespace SimpleAuthentication
{
    public class AccessToken
    {
        public DateTime Expires { get; }
        public string Token { get; }
        public string RefreshToken { get; }
        public DateTime RefreshTokenExpiryTime { get;  }
        internal AccessToken(string token, DateTime expires, string refreshToken, DateTime refreshTokenExpiryTime)
        {
            Expires = expires;
            Token = token;
            RefreshToken=refreshToken;
            RefreshTokenExpiryTime=refreshTokenExpiryTime;
        }
    }
}