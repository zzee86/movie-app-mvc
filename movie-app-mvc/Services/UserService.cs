using MovieApp.Data.Models;
using MovieApp.Data.Context;
using movie_app_mvc.Models.Users;

namespace MovieApp.Services
{
    public class UserService
    {
        public async Task<UserViewModel> GetUser(int userId)
        {
            using (MovieDbContext _movieDbContext = new MovieDbContext())
            {
                return _movieDbContext.Users
                    .Select(u => new UserViewModel
                    {
                        Email = u.Email,
                        Username = u.Username
                    })
                    .FirstOrDefault(u => u.Id == userId);
            }
        }



        public async Task CreateUser(CreateUser createUser)
        {
            using (MovieDbContext _movieDbContext = new MovieDbContext())
            {
                await _movieDbContext.Users.AddAsync(new User
                {
                    Email = createUser.Email,
                    Username = createUser.Username,
                    Password = createUser.Password
                });

                _movieDbContext.SaveChanges();
            }
        }
    }
}