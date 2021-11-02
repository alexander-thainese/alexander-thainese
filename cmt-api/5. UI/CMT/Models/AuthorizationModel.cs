using CMT.Common;
using CMT.Models;
using System;

namespace CMT
{
    public class AuthorizationModel
    {

        public static bool ValidateUser(string userName, string password)
        {
            if (userName == "testUser" && password == "Password123")
            {
                return true;
            }
            return false;

        }

        public static bool ValidateToken(Guid token)
        {
            object existingToken = MemoryCacher.GetValue(token.ToString());
            if (existingToken != null)
            {
                return true;
            }
            return false;

        }

        public static TokenContainer CreateToken(string user)
        {

            TokenContainer token = new TokenContainer() { AccessToken = Guid.NewGuid().ToString(), Expires = DateTime.Now.AddMinutes(120), UserName = user };
            MemoryCacher.Add(token.AccessToken, token, new DateTimeOffset(DateTime.Now.AddMinutes(120)));
            return token;
        }
    }
}