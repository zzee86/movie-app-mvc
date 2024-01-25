//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MovieApp.Service.APIModels.Users;

namespace movie_app_mvc.Controllers
{
    public interface ILoginController
    {
        IActionResult Index();
        Task<IActionResult> Login(LoginUser loginUser);
        Task<IActionResult> Logout();
        IActionResult Register();
        Task<IActionResult> Register(CreateUser createUser);
        Task<RedirectToActionResult> SetupCookies(string userEmail);
    }
}