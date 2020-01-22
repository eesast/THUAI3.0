using Communication.Proto;
using Communication.RestServer.Services.Interfaces;
using System;
using System.Collections.Generic;

namespace Communication.RestServer.Services
{
    //TODO: use database implement
    internal class AuthService : IAuthService
    {
        private readonly TimeSpan expireTime = new TimeSpan(0, 0, Constants.TokenExpire);
        private Dictionary<string, Tuple<string, string>> accounts = new Dictionary<string, Tuple<string, string>>();
        private Dictionary<string, Tuple<string, DateTime>> tokens = new Dictionary<string, Tuple<string, DateTime>>();

        private static string GenerateToken()
        {
            return Guid.NewGuid().ToString();
        }

        public void CreateUser(string username, string password)
        {
            accounts[username] = new Tuple<string, string>(password, "");
        }

        //This method should be limited usage.
        public string GetToken(string username, string password)
        {
            string token = GenerateToken();
            //TODO: move this specific situation to the database initialization.
            if (username == "admin" && password == "admin")
            {
                tokens.Add(token, new Tuple<string, DateTime>(username, DateTime.Now));
                return token;
            }
            if (!accounts.TryGetValue(username, out Tuple<string, string> accountInfo)) return null;
            if (accountInfo.Item1 != password) return null;
            if (accountInfo.Item2 != "")
                tokens.Remove(accountInfo.Item2);
            accounts[username] = new Tuple<string, string>(password, token);
            tokens.Add(token, new Tuple<string, DateTime>(username, DateTime.Now));
            return token;
        }

        public string GetUser(string token)
        {
            if (!tokens.TryGetValue(token, out Tuple<string, DateTime> tokenInfo)) return null;
            if (DateTime.Now - tokenInfo.Item2 > expireTime) return null;
            return tokenInfo.Item1;
        }
    }
}
