using System.ComponentModel.DataAnnotations;

namespace movie_app_mvc.Models.Users
{
    public class LoginUser
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }
}
