using System;
using static movie_app_mvc.Models.MoviePGRating;

namespace movie_app_mvc.Models
{
    public class TvPGRating
    {
        // URL https://api.themoviedb.org/3/tv/1399/content_ratings?api_key=ca80dfbe1afe5a1a97e4401ff534c4e4
        public class Result
        {
            public List<string> descriptors { get; set; }
            public string iso_3166_1 { get; set; }
            public string rating { get; set; }
        }

        public class Root
        {
            public List<Result> results { get; set; }
            public int id { get; set; }
        }
    }
}

