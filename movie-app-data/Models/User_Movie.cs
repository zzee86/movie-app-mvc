using Microsoft.Exchange.WebServices.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace movie_app_data.Models
{
    public class User_Movie
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Movie_User_Id { get; set; }

        [Required]
        [ForeignKey("UserId")]
        public User UserId;

        [Required]
        [ForeignKey("MovieId")]
        public Movie MovieId;
    }
}