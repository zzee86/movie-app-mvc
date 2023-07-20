using System;
namespace movie_app_mvc.Models
{
    public class MediaWatchProviders
    {
        public class Buy
        {
            public string logo_path { get; set; }
            public int provider_id { get; set; }
            public string provider_name { get; set; }
            public int display_priority { get; set; }

            public string logo_path_url => string.Format("https://image.tmdb.org/t/p/original{0}", logo_path);
        }

        public class GB
        {
            public string link { get; set; }
            public List<Buy> buy { get; set; }

            public List<Rent> rent { get; set; }

            public List<Flatrate> flatrate { get; set; }

        }

        public class Rent
        {
            public string logo_path { get; set; }
            public int provider_id { get; set; }
            public string provider_name { get; set; }
            public int display_priority { get; set; }
        }

        public class Results
        {
            public GB GB { get; set; }
            public US US { get; set; }
        }

        public class Root
        {
            public int id { get; set; }
            public Results results { get; set; }
        }

        public class US
        {
            public string link { get; set; }
            public List<Rent> rent { get; set; }
            public List<Buy> buy { get; set; }

            public List<Flatrate> flatrate { get; set; }

        }
        public class Flatrate
        {
            public string logo_path { get; set; }
            public int provider_id { get; set; }
            public string provider_name { get; set; }
            public int display_priority { get; set; }

            public string logo_path_url => string.Format("https://image.tmdb.org/t/p/original{0}", logo_path);

        }
    }
}

