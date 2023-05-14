namespace SimpleAuthentication
{
    public class RefreshTokenRequest
    {
        public string Token { get; internal set; }
        public object RefreshToken { get; internal set; }
    }
}