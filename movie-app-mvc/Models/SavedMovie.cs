using System;
using System.ComponentModel.DataAnnotations;

namespace movie_app_mvc.Models
{
	public class SavedMovie
	{
        [Key]
        public int Id { get; set; }

        public string Title { get; set; }

        public string Overview { get; set; }

        public string Poster { get; set; }

        public bool IsSaved { get; set; }

    }
}

