using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace movie_app_data.Models
{
    public class Movie
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int MovieId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(200)]
        public string Poster { get; set; }

        [Required]
        public DateTime Created { get; set; }

        [Required]
        public int TheMovieDbId { get; set; }

        [Required]
        public ICollection<User_Movie> UserMovies { get; set; }
    }
}
