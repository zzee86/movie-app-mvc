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

namespace movie_app_mvc.Controllers
{
    public class LoginController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public LoginController()
        {
            _connectionString = "server=localhost;database=saved_movies;user=root;";
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string email, string password)
        {
            // Validate login credentials
            if (ValidateLogin(email, password))
            {
                // Set authentication cookie with user's name
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, email) // Replace with the appropriate claim type and value
        };
                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);
                HttpContext.SignInAsync(principal).Wait();

                // Successful login
                return RedirectToAction("Index", "Home");
            }

            // Invalid credentials
            ViewData["ErrorMessage"] = "Invalid email or password.";
            return View("Index");
        }


        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string email, string username, string password)
        {
            // Check if the email already exists
            if (IsEmailExists(email))
            {
                ViewData["ErrorMessage"] = "Email already registered.";
                return View();
            }

            // Register the new user
            if (CreateUser(email, username, password))
            {
                // Successful registration
                return RedirectToAction("Index", "Home");
            }

            // Failed to register
            ViewData["ErrorMessage"] = "Failed to register user.";
            return View();
        }

        private bool ValidateLogin(string email, string password)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                var query = "SELECT COUNT(*) FROM loginDetails WHERE email = @Email AND password = @Password";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Password", password);

                    var result = Convert.ToInt32(command.ExecuteScalar());
                    return result > 0;
                }
            }
        }

        private bool IsEmailExists(string email)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                var query = "SELECT COUNT(*) FROM loginDetails WHERE email = @Email";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);

                    var result = Convert.ToInt32(command.ExecuteScalar());
                    return result > 0;
                }
            }
        }

        private bool CreateUser(string email, string username, string password)
        {
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();

                var query = "INSERT INTO loginDetails (email, username, password) VALUES (@Email, @Username, @Password)";
                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@Username", username);
                    command.Parameters.AddWithValue("@Password", password);

                    var rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }
    }
}