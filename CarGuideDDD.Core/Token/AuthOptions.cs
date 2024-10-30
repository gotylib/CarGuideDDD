using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace CarGuideDDD.Core.Token
{
    public abstract class AuthOptions
    {
        public const string Issuer = "MyAuthServer";
        public const string Audience = "MyAuthClient";
        const string Key = "A1b2C3d4E5f6G7h8I9j0K1l2M3n4O5p4g7h8j9k0l1m2n3o4p5q6r7s8t9u0v1wThisIsASecretKey12345678901234";
        public const int Lifetime = 1;
        public static SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Key));
        }
    }
}
