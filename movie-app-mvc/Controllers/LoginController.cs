//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Configuration;
//using MySql.Data.MySqlClient;
//using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
//using movie_app_mvc.Models;
//using MovieApp.Data.Context;
//using MovieApp.Data.Models;
using MovieApp.Services.Interfaces;
using MovieApp.Services.APIModels.Users;
using MovieApp.Services;
//using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace movie_app_mvc.Controllers
{
    public class LoginController : Controller, ILoginController
    {
        private readonly IUserService UserService;
        public LoginController(IUserService userService)
        {
            this.UserService = userService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginUser loginUser)
        {
            try
            {
                UserService.LoginUser(loginUser);
                return await SetupCookies(loginUser.Email);
            }
            catch (DuplicateUserException ex)
            {
                TempData["LoginErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
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
                await UserService.CreateUser(createUser);
                return await SetupCookies(createUser.Email);
            }
            catch (DuplicateUserException ex)
            {
                TempData["RegisterErrorMessage"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        public async Task<RedirectToActionResult> SetupCookies(string userEmail)
        {
            var cookieDetails = UserService.SetupCookies(userEmail);
            await HttpContext.SignInAsync(cookieDetails);

            return RedirectToAction("Index", "Home");
        }
    }
}