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

        public async Task LoginUser(LoginUser loginUser)
        {
            using (MovieDbContext _movieDbContext = new MovieDbContext())
            {

                if (!ValidateLogin(loginUser.Email, loginUser.Password))
                {
                    throw new DuplicateUserException("Invalid email or password.");
                }
            }
        }

        private bool ValidateLogin(string email, string password)
        {
            using (MovieDbContext _movieDbContext = new MovieDbContext())
            {
                bool loginValid = _movieDbContext.Users.Any(u => u.Email == email && u.Password == password);

                return loginValid;
            }
        }

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

    public class DuplicateUserException : Exception
    {
        public DuplicateUserException(string message) : base(message)
        {
        }
    }
}