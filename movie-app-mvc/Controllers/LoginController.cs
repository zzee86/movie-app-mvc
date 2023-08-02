using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using movie_app_mvc.Models;
using MovieApp.Data.Context;
using MovieApp.Data.Models;
using movie_app_mvc.Models.Users;
using MovieApp.Services;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace movie_app_mvc.Controllers
{
    public class LoginController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(User user)
        {
            // Validate login credentials
            if (ValidateLogin(user.Email, user.Password))
            {
              return await SetupCookies(user.Email);
            }

            // Invalid credentials
            TempData["LoginErrorMessage"] = "Invalid email or password.";
            return View("Index");
        }


        public async Task<IActionResult> Logout()
        {
            // Perform the logout logic
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(CreateUser createUser)
        {
            try
            {
                var userService = new UserService();
                await userService.CreateUser(createUser);

                return await SetupCookies(createUser.Email);
            }
            catch (DuplicateUserException ex)
            {
                TempData["RegisterErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
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
        public async Task<RedirectToActionResult> SetupCookies(string userEmail)
        {
            var claim = new[]
{
                    new Claim(ClaimTypes.Name, userEmail)
                };

            var identity = new ClaimsIdentity(claim, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(principal);

            // Successful login
            return RedirectToAction("Index", "Home");
        }
    }
}