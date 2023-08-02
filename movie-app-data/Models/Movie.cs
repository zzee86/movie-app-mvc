using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.Data.Models
{
    public class Movie : EntityBase
    {
        [Required]
        public int MovieDbId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(200)]
        public string Poster { get; set; }

        [Required]
        public DateTime Created { get; set; }

        public ICollection<User> Users { get; set; }
    }
}
