using System.ComponentModel.DataAnnotations;

namespace movie_app_mvc.Models.Users
{
    public class CreateUser
    {
        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(50)]
        public string Password { get; set; }
    }
}
