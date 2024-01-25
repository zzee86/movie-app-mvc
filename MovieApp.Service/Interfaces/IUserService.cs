using MovieApp.Service.APIModels.Users;

namespace MovieApp.Service.Interfaces
{
    public interface IUserService
    {
        Task CreateUser(CreateUser createUser);
        Task LoginUser(LoginUser loginUser);
    }
}