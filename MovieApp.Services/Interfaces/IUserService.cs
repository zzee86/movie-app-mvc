using MovieApp.Services.APIModels.Users;

namespace MovieApp.Services.Interfaces
{
    public interface IUserService
    {
        Task CreateUser(CreateUser createUser);
        Task LoginUser(LoginUser loginUser);
    }
}