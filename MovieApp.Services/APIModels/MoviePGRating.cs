using System;
namespace MovieApp.Services.APIModels
{
    public class MoviePGRating
    {
        // URL https://api.themoviedb.org/3/movie/575264/release_dates?api_key=ca80dfbe1afe5a1a97e4401ff534c4e4
        public class ReleaseDate
        {
            public string certification { get; set; }
            public List<object> descriptors { get; set; }
            public string iso_639_1 { get; set; }
            public string note { get; set; }
            public DateTime release_date { get; set; }
            public int type { get; set; }
        }

        public class Result
        {
            public string iso_3166_1 { get; set; }
            public List<ReleaseDate> release_dates { get; set; }
        }

        public class Root
        {
            public int id { get; set; }
            public List<Result> results { get; set; }
        }
    }
}

