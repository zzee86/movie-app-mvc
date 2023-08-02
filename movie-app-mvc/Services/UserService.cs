using MovieApp.Data.Models;
using MovieApp.Data.Context;
using movie_app_mvc.Models.Users;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

namespace MovieApp.Services
{
    public class UserService
    {
        //public async Task<UserViewModel> GetUser(int userId)
        //{
        //    using (MovieDbContext _movieDbContext = new MovieDbContext())
        //    {
        //        return _movieDbContext.Users
        //            .Select(u => new UserViewModel
        //            {
        //                Email = u.Email,
        //                Username = u.Username
        //            })
        //            .FirstOrDefault(u => u.Id == userId);
        //    }
        //}



        public async Task CreateUser(CreateUser createUser)
        {
            using (MovieDbContext _movieDbContext = new MovieDbContext())
            {

                if (IsUserExists(createUser.Email, createUser.Username))
                {
                    throw new DuplicateUserException("Email or Username already registered.");
                }

                await _movieDbContext.Users.AddAsync(new User
                {
                    Email = createUser.Email,
                    Username = createUser.Username,
                    Password = createUser.Password
                });

                _movieDbContext.SaveChanges();
            }
        }

        private bool IsUserExists(string email, string username)
        {
            using (MovieDbContext _movieDbContext = new MovieDbContext())
            {
                bool userexists = _movieDbContext.Users.Any(u => u.Email == email || u.Username == username);

                return userexists;
            }
        }
    }

    // Custom exception to handle duplicate user registration
    public class DuplicateUserException : Exception
    {
        public DuplicateUserException(string message) : base(message)
        {
        }
    }
}