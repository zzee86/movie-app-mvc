using System;
namespace MovieApp.Service.APIModels
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
            public string poster_path_url => string.Format("https://image.tmdb.org/t/p/original{0}", poster_path);
            public string backdrop_path_url => string.Format("https://image.tmdb.org/t/p/original{0}", backdrop_path);
            //public string poster_path_url => string.IsNullOrEmpty(poster_path) ? "no_image.png" : $"https://image.tmdb.org/t/p/w185{poster_path}";
            public bool IsSaved { get; set; }





            public List<KnownFor> known_for { get; set; }




            // For video
            public string key { get; set; }

            public string type { get; set; }

            public static explicit operator List<object>(Result? v)
            {
                throw new NotImplementedException();
            }
        }



        public class KnownFor
        {
            public bool IsSaved { get; set; }

            public bool adult { get; set; }
            public string backdrop_path { get; set; }
            public int id { get; set; }
            public string title { get; set; }
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
            public string name { get; set; }
            public string original_name { get; set; }
            public string first_air_date { get; set; }
            public List<string> origin_country { get; set; }
        }




        public class Root
        {
            public int page { get; set; }
            public List<Result> results { get; set; }
            public int total_pages { get; set; }

        }
    }
}


