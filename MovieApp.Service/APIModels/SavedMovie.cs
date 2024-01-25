using System;
using System.ComponentModel.DataAnnotations;

namespace MovieApp.Services.APIModels
{
    public class SavedMovie
    {
        public int Id { get; set; }
        public int MovieDbId { get; set; }

        public string Title { get; set; }

        public string Poster { get; set; }

        public DateTime Created { get; set; }

        public double Rating { get; set; }
    }
}

