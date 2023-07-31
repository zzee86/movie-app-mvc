﻿using System;
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
using movie_app_data.Models;

namespace movie_app_mvc.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString = "server=localhost;database=saved_movies;user=root;";

        //public LoginController()
        //{
        //    _connectionString = "server=localhost;database=saved_movies;user=root;";
        //}

        // GET: /<controller>/
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

                var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                await HttpContext.SignInAsync(principal);

                // Successful login
                return RedirectToAction("Index", "Home");
            }

            // Invalid credentials
            ViewData["ErrorMessage"] = "Invalid email or password.";
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
        public IActionResult Register(User user)
        {
            // Check if the email already exists
            if (IsEmailExists(user.Email))
            {
                TempData["ErrorMessage"] = "Email already registered.";
                return RedirectToAction("Index");
            }
            try
            {
                using (MovieDbContext _movieDbContext = new MovieDbContext())
                {
                    _movieDbContext.Users.Add(user);
                    _movieDbContext.SaveChanges();
                    return RedirectToAction("Index", "Home");
                }
            }
            catch
            {
                TempData["ErrorMessage"] = "Email already registered.";
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

        private bool IsEmailExists(string email)
        {
            using (MovieDbContext _movieDbContext = new MovieDbContext())
            {
                bool emailExists = _movieDbContext.Users.Any(u => u.Email == email);
                
                return emailExists;
            }
        }

        private string GetUsernameByEmail(string email)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                var query = "SELECT username FROM loginDetails WHERE email = @Email";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return reader.GetString("username");
                        }
                    }
                }
            }

            // Return null if the email does not have an associated username
            return null;
        }

    }
}