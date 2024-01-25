using System.ComponentModel.DataAnnotations;

namespace MovieApp.Services.APIModels.Users
{
    public class UserViewModel
    {
        public string Email { get; set; }

        public string Username { get; set; }
    }
}
