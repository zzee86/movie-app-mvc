using System.Security.Claims;
using MovieApp.Services.APIModels.Users;

namespace MovieApp.Services.Interfaces
{
    public interface IUserService
    {
        Task CreateUser(CreateUser createUser);
        void LoginUser(LoginUser loginUser);
        bool ValidateLogin(string email, string password);
        ClaimsPrincipal SetupCookies(string userEmail);
    }
}