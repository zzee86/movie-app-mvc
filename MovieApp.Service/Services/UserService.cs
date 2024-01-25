
using Microsoft.AspNetCore.Authentication.Cookies;
using MovieApp.Data.Context;
using MovieApp.Data.Models;
using MovieApp.Service.APIModels.Users;
using MovieApp.Service.Interfaces;
using System.Security.Claims;

namespace MovieApp.Service.Services
{
    public class UserService : IUserService
    {
        private IMovieDbContext MovieDbContext { get; set; }

        public UserService(IMovieDbContext movieDbContext)
        {
            this.MovieDbContext = movieDbContext;
        }

        public async Task LoginUser(LoginUser loginUser)
        {
            // test
            //using (MovieDbContext _movieDbContext = new MovieDbContext())
            //{

            if (!ValidateLogin(loginUser.Email, loginUser.Password))
            {
                throw new DuplicateUserException("Invalid email or password.");
            }
        }

        private bool ValidateLogin(string email, string password)
        {
            bool loginValid = MovieDbContext.Users.Any(u => u.Email == email && u.Password == password);

            return loginValid;
        }

        public async Task CreateUser(CreateUser createUser)
        {
            if (IsUserExists(createUser.Email, createUser.Username))
            {
                throw new DuplicateUserException("Email or Username already registered.");
            }

            await MovieDbContext.Users.AddAsync(new User
            {
                Email = createUser.Email,
                Username = createUser.Username,
                Password = createUser.Password
            });

            MovieDbContext.SaveChanges();
        }

        private bool IsUserExists(string email, string username)
        {
            bool userexists = MovieDbContext.Users.Any(u => u.Email == email || u.Username == username);
            return userexists;
        }
    }

    public class DuplicateUserException : Exception
    {
        public DuplicateUserException(string message) : base(message)
        {
        }
    }
}