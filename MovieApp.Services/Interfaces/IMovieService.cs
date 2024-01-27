using Microsoft.AspNetCore.Mvc;
using MovieApp.Services.APIModels;

namespace MovieApp.Services.Interfaces
{
    public interface IMovieService
    {
        Task<MovieInfo.Result> FetchMovie(string url);
        Task<MovieInfo.Root> FetchMovies(string url);
        List<MovieInfo.Result> ProcessMovieResults(List<MovieInfo.Result> results, bool isUserAuthenticated);
        IActionResult ReloadCurrentUrl();
    }
}