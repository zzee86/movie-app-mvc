using System.ComponentModel.DataAnnotations;

namespace MovieApp.Service.APIModels.Users
{
    public class LoginUser
    {
        public string Email { get; set; }

        public string Password { get; set; }
    }
}
