using System;
namespace MovieApp.Services.APIModels
{
    public class Actors
	{
        public class Cast
        {
            public bool adult { get; set; }
            public int gender { get; set; }
            public int id { get; set; }
            public string known_for_department { get; set; }
            public string name { get; set; }
            public string original_name { get; set; }
            public double popularity { get; set; }
            public string profile_path { get; set; }
            public string profile_path_url => string.Format("https://image.tmdb.org/t/p/original{0}", profile_path);

            public int cast_id { get; set; }
            public string character { get; set; }
            public string credit_id { get; set; }
            public int order { get; set; }
        }

        public class Crew
        {
            public bool adult { get; set; }
            public int gender { get; set; }
            public int id { get; set; }
            public string known_for_department { get; set; }
            public string name { get; set; }
            public string original_name { get; set; }
            public double popularity { get; set; }
            public string profile_path { get; set; }
            public string credit_id { get; set; }
            public string department { get; set; }
            public string job { get; set; }
        }

        public class Root
        {
            public int id { get; set; }
            public List<Cast> cast { get; set; }
            public List<Crew> crew { get; set; }
        }

    }
}

