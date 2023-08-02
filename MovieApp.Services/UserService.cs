using MovieApp.Data.Models;
using MovieApp.Data.Context;

namespace MovieApp.Services
{
    public class UserService
    {
        // crud 
        public Task CreateUser()
        {
            using (MovieDbContext _movieDbContext = new MovieDbContext())
            {
                _movieDbContext.Users.
            }
        }
    }
}