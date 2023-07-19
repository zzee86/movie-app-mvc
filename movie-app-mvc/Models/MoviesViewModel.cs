using System;
namespace movie_app_mvc.Models
{
	public class MoviesViewModel
    {
        public List<MovieInfo.Result> TrendingMovies { get; set; }
        public List<MovieInfo.Result> PopularMovies { get; set; }
        public List<MovieInfo.Result> TopRatedMovies { get; set; }

        public List<MovieInfo.Result> RecommendedMovies { get; set; }
    }
}

