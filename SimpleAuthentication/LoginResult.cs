namespace SimpleAuthentication
{
    public class LoginResult : Result
    {
        public Guid Key { get; }
        private LoginResult(bool success,Guid key, string message):base(success,message)
        {
              Key = key;
        }
        internal static LoginResult Success(Guid key, string message = "")
        {
            return new LoginResult(true,key,message);
        }
        internal static new LoginResult Failure(string message = "")
        {
            return new LoginResult(false,Guid.Empty,message);
        }
    }
}
