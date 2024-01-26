using System;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MovieApp.Data.Context;
using MovieApp.Data.Models;
using MovieApp.Services.APIModels;
using MovieApp.Services.Interfaces;
using Newtonsoft.Json;

namespace MovieApp.Services.Services
{
    public class MovieService : IMovieService
    {
        private readonly IUserService UserService;
        private readonly IMovieDbContext MovieDbContext;
        private readonly IHttpContextAccessor HttpContextAccessor;

        public MovieService(IUserService userService, IMovieDbContext movieDbContext, IHttpContextAccessor httpContextAccessor)
        {
            this.UserService = userService;
            this.MovieDbContext = movieDbContext;
            HttpContextAccessor = httpContextAccessor;
        }

        public async Task<MovieInfo.Result> FetchMovie(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var info = JsonConvert.DeserializeObject<MovieInfo.Result>(json);

                return info;
            }
        }
        public async Task<MovieInfo.Root> FetchMovies(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var info = JsonConvert.DeserializeObject<MovieInfo.Root>(json);

                return info;
            }
        }

        public List<MovieInfo.Result> ProcessMovieResults(List<MovieInfo.Result> results, bool isUserAuthenticated)
        {
            List<MovieInfo.Result> movieResults = new List<MovieInfo.Result>();

            foreach (var movie in results)
            {
                if (string.IsNullOrEmpty(movie.title))
                {
                    // Use the name property if title is empty
                    movie.title = movie.name;
                }

                if (string.IsNullOrEmpty(movie.poster_path) && (movie.known_for == null || movie.known_for.Count == 0))
                {
                    // Exclude the movie if it doesn't have a poster and no known_for data
                    continue;
                }

                if (movie.known_for != null && movie.known_for.Count > 0)
                {
                    bool hasValidKnownFor = movie.known_for.Any(k => !string.IsNullOrEmpty(k.title) && !string.IsNullOrEmpty(k.poster_path));

                    if (hasValidKnownFor)
                    {
                        // Filter out the movie if it has a valid known_for
                        continue;
                    }
                }

                if (isUserAuthenticated)
                {
                    movie.IsSaved = MovieIsSaved(movie.id);
                }
                movieResults.Add(movie);
            }

            return movieResults;
        }

        private bool MovieIsSaved(int movieDbId)
        {
            string userEmail = HttpContextAccessor.HttpContext.User.Identity?.Name ?? string.Empty;
            User user = MovieDbContext.Users.FirstOrDefault(u => u.Email == userEmail) ?? throw new Exception("User not found");
            Movie? movie = MovieDbContext.Movies.Include(x => x.Users).FirstOrDefault(u => u.MovieDbId == movieDbId);
            bool isSaved = movie != null && movie.Users.Contains(user);
            return isSaved;

        }
    }
}

