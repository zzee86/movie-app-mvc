using System.ComponentModel.DataAnnotations;

namespace MovieApp.Data.Models
{
    public class User : EntityBase
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

        public ICollection<Movie> Movies { get; set; }

    }
}
