
namespace Communication.RestServer.Services.Interfaces
{
    interface IAuthService
    {
        void CreateUser(string username, string password);
        string GetToken(string username, string password);
        string GetUser(string token);
    }
}
