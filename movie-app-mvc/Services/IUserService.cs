using movie_app_mvc.Models.Users;

namespace MovieApp.Services
{
    public interface IUserService
    {
        Task CreateUser(CreateUser createUser);
        Task LoginUser(LoginUser loginUser);
    }
}