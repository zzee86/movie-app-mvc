using System;
namespace movie_app_mvc.Models
{
    public class MovieInfo
    {
        public class Result
        {
            public bool adult { get; set; }
            public string backdrop_path { get; set; }
            public int id { get; set; }
            public string title { get; set; }
            public string name { get; set; }
            public string original_language { get; set; }
            public string original_title { get; set; }
            public string overview { get; set; }
            public string poster_path { get; set; }
            public string media_type { get; set; }
            public List<int> genre_ids { get; set; }
            public double popularity { get; set; }
            public string release_date { get; set; }
            public bool video { get; set; }
            public double vote_average { get; set; }
            public int vote_count { get; set; }
            public string poster_path_url => string.Format("https://image.tmdb.org/t/p/w185{0}", poster_path);
            //public string poster_path_url => string.IsNullOrEmpty(poster_path) ? "no_image.png" : $"https://image.tmdb.org/t/p/w185{poster_path}";
            public bool IsSaved { get; set; }


        }

        public class Root
        {
            public int page { get; set; }
            public List<Result> results { get; set; }
            public int total_pages { get; set; }

        }
    }
}


