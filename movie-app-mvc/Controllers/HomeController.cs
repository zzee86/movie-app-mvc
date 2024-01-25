using Microsoft.AspNetCore.Mvc;
//using MySql.Data.MySqlClient;
using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Threading.Tasks;
using movie_app_mvc.Models;
//using System.Web;
using Microsoft.EntityFrameworkCore;
//using static movie_app_mvc.Models.TvShowDetails;
//using System.Net;
//using ColorThiefDotNet;
//using System.Drawing;
//using static System.Net.WebRequestMethods;
//using System.Linq;
//using Microsoft.AspNetCore.Mvc.Formatters;
using MovieApp.Data.Context;
using MovieApp.Data.Models;
using System.Drawing.Printing;
//using movie_app_mvc.Models.Users;
//using Microsoft.EntityFrameworkCore.Internal;

namespace movie_app_mvc.Controllers
{
    public class HomeController : Controller
    {
        private IMovieDbContext MovieDbContext { get; set; }
        public HomeController(IMovieDbContext movieDbContext)
        {
            this.MovieDbContext = movieDbContext;
        }

        public async Task<IActionResult> Index(string searchQuery, int page = 1)
        {
            // Get the user ID of the logged-in user
            string email = User.Identity.Name; // Assuming the email is stored in the "Name" claim

            // Retrieve the user ID from the loginDetails table
            string testing = "t";
            TempData["CurrentDateTime"] = DateTime.Now.ToString();

            List<MovieInfo.Result> trendingMovies = new List<MovieInfo.Result>();
            List<MovieInfo.Result> popularMovies = new List<MovieInfo.Result>();
            List<MovieInfo.Result> topRatedMovies = new List<MovieInfo.Result>();

            if (string.IsNullOrEmpty(searchQuery))
            {
                trendingMovies = await LoadMovies(page, testing);
                popularMovies = await LoadPopularMovies(page, testing);
                topRatedMovies = await LoadTopRatedMovies(page, testing);
            }
            else
            {
                trendingMovies = await SearchMovies(searchQuery, testing, page);
            }

            ViewBag.CurrentPage = page;
            ViewBag.SearchQuery = searchQuery;

            var viewModel = new MoviesViewModel
            {
                TrendingMovies = trendingMovies,
                PopularMovies = popularMovies,
                TopRatedMovies = topRatedMovies
            };
            return View(viewModel);
        }


        public async Task<List<MovieInfo.Result>> LoadMovies(int page, string userID)
        {
            string apiUrl = $"https://api.themoviedb.org/3/trending/all/day?language=en-US&api_key={Constants.Constants.apiKey}&page={page}";
            MovieInfo.Root movieInfo = await FetchMovies(apiUrl);

            if (movieInfo != null)
            {
                List<MovieInfo.Result> movieResults = ProcessMovieResults(movieInfo.results, userID);

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = movieInfo.total_pages;
                //ViewBag.TotalPages = (int)Math.Ceiling((double)movieInfo.total_results / PageSize);

                return movieResults;
            }

            return null;
        }

        public async Task<List<MovieInfo.Result>> LoadPopularMovies(int page, string userID)
        {
            string apiUrl = $"https://api.themoviedb.org/3/movie/popular?language=en-US&api_key={Constants.Constants.apiKey}&page={page}";
            MovieInfo.Root movieInfo = await FetchMovies(apiUrl);

            if (movieInfo != null)
            {
                List<MovieInfo.Result> movieResults = ProcessMovieResults(movieInfo.results, userID);

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = movieInfo.total_pages;
                //ViewBag.TotalPages = (int)Math.Ceiling((double)movieInfo.total_results / PageSize);

                return movieResults;
            }

            return null;
        }

        public async Task<List<MovieInfo.Result>> LoadTopRatedMovies(int page, string userID)
        {
            string apiUrl = $"https://api.themoviedb.org/3/movie/top_rated?language=en-US&api_key={Constants.Constants.apiKey}&page={page}";
            MovieInfo.Root movieInfo = await FetchMovies(apiUrl);

            if (movieInfo != null)
            {
                List<MovieInfo.Result> movieResults = ProcessMovieResults(movieInfo.results, userID);

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = movieInfo.total_pages;
                //ViewBag.TotalPages = (int)Math.Ceiling((double)movieInfo.total_results / PageSize);

                return movieResults;
            }

            return null;
        }

        private async Task<MovieInfo.Root> FetchMovies(string url)
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

        private async Task<MovieInfo.Result> FetchMovie(string url)
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

        private List<MovieInfo.Result> ProcessMovieResults(List<MovieInfo.Result> results, string userID)
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

                if (User.Identity.IsAuthenticated)
                {
                    movie.IsSaved = MovieIsSaved(movie.id);
                }
                movieResults.Add(movie);

                if (results.Count() >= 1)
                {
                    TempData["ProcessMovieCount"] = "Active";
                }
            }

            return movieResults;
        }


        private async Task<List<MovieInfo.Result>> SearchMovies(string searchQuery, string userID, int pageNumber = 1)
        {
            string url = $"https://api.themoviedb.org/3/search/multi?language=en-US&api_key={Constants.Constants.apiKey}&query={searchQuery}&page={pageNumber}";
            MovieInfo.Root movieInfo = await FetchMovies(url);

            if (movieInfo != null)
            {
                List<MovieInfo.Result> movieResults = ProcessMovieResults(movieInfo.results, userID);

                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = movieInfo.total_pages;
                ViewBag.SearchQuery = searchQuery;

                return movieResults;
            }

            return null;
        }

        public IActionResult ReloadCurrentUrl()
        {

            // Determine the referring URL to refresh the current page
            string referringUrl = Request.Headers["Referer"].ToString();
            if (string.IsNullOrEmpty(referringUrl))
            {
                // If the referring URL is empty, redirect to the home page
                return RedirectToAction("Index");
            }
            else
            {
                // Redirect back to the referring URL
                return Redirect(referringUrl);
            }
        }

        public IActionResult SavedMovies(string title, int page = 1)
        {
            string userEmail = User.Identity.Name; // Assuming the email is stored in the "Name" claim

            if (userEmail == null)
            {
                // User ID not found
                // Handle the error or redirect as needed
                return RedirectToAction("Index");
            }

            // int pageSize = 28;
            List<SavedMovie> savedMovies = GetMoviesFromDatabase(title, userEmail, page, Constants.Constants.PageSize);
            ViewBag.SavedPage = page;
            ViewBag.SearchQuery = title;

            int totalCount = GetTotalMovieCount(title, userEmail);

            int totalPages = (int)Math.Ceiling((double)totalCount / Constants.Constants.PageSize);
            ViewBag.TotalPages = totalPages;

            bool hasNextPage = (page * Constants.Constants.PageSize) < totalCount;

            ViewBag.HasNextPage = hasNextPage;
            ViewBag.NextPage = page + 1;
            ViewBag.PreviousPage = page - 1;

            return View(savedMovies);
        }

        private int GetTotalMovieCount(string title, string userEmail)
        {
            var query = MovieDbContext.Movies.Where(m => m.Users.Any(u => u.Email == userEmail));
            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(m => m.Title.Contains(title));
            }
            return query.Count();
        }


        public IActionResult RemoveMovie(int MovieDbId, string searchQuery, int page = 1)
        {
            string userEmail = User.Identity?.Name ?? string.Empty;
            User user = MovieDbContext.Users.FirstOrDefault(u => u.Email == userEmail) ?? throw new Exception("User not found");

            var movieToRemove = MovieDbContext.Movies.Include(x => x.Users).FirstOrDefault(x => x.MovieDbId == MovieDbId);

            if (movieToRemove != null)
            {
                // Remove association with user
                if (movieToRemove.Users.Contains(user))
                {
                    movieToRemove.Users.Remove(user);
                }

                // If no user reference with movie then remove it
                bool isReferenced = movieToRemove.Users.Any();
                if (!isReferenced)
                {
                    MovieDbContext.Movies.Remove(movieToRemove);
                }
                MovieDbContext.SaveChanges();

            }
            return ReloadCurrentUrl();
        }

        private List<SavedMovie> GetMoviesFromDatabase(string title, string userEmail, int page, int pageSize)
        {
            List<SavedMovie> movies;
            var query = MovieDbContext.Movies.Where(m => m.Users.Any(u => u.Email == userEmail));

            if (!string.IsNullOrEmpty(title))
            {
                query = query.Where(m => m.Title.Contains(title));
            }
            movies = query.OrderByDescending(m => m.Created)
               .Skip((page - 1) * pageSize)
               .Take(pageSize)
               .Select(m => new SavedMovie
               {
                   Id = m.Id,
                   Title = m.Title,
                   MovieDbId = m.MovieDbId,
                   Poster = m.Poster,
                   Rating = m.Rating,
                   Created = m.Created,
               })
               .ToList();

            return movies;
        }

        private bool MovieIsSaved(int movieDbId)
        {
            string userEmail = User.Identity?.Name ?? string.Empty;
            User user = MovieDbContext.Users.FirstOrDefault(u => u.Email == userEmail) ?? throw new Exception("User not found");
            Movie? movie = MovieDbContext.Movies.Include(x => x.Users).FirstOrDefault(u => u.MovieDbId == movieDbId);
            bool isSaved = movie != null && movie.Users.Contains(user);
            return isSaved;

        }

        public async Task<IActionResult> SaveMovie(int movieDbId, string Title)
        {
            try
            {
                string userEmailt = User.Identity?.Name ?? string.Empty;

                User usertemp = MovieDbContext.Users.FirstOrDefault(u => u.Email == userEmailt) ?? throw new Exception("User not found");
                string userEmail = User.Identity?.Name ?? string.Empty;
                User user = MovieDbContext.Users.FirstOrDefault(u => u.Email == userEmail) ?? throw new Exception("User not found");
                Movie? movie = MovieDbContext.Movies.FirstOrDefault(m => m.MovieDbId == movieDbId);

                string apiUrlExtra = $"https://api.themoviedb.org/3/search/multi?language=en-US&api_key={Constants.Constants.apiKey}&query={Title}";
                MovieInfo.Root movieExtra = await FetchMovies(apiUrlExtra);
                MovieInfo.Result movieDetailsResult = movieExtra.results.FirstOrDefault();
                string media_type = (movieDetailsResult.media_type == "movie") ? "movie" : "tv";


                if (movie != null)
                {
                    movie.Users = new List<User> { user };
                    movie.Users.Add(user);
                }
                else
                {
                    string apiUrl = $"https://api.themoviedb.org/3/{media_type}/{movieDbId}?api_key={Constants.Constants.apiKey}";
                    MovieInfo.Result movieFromApi = await FetchMovie(apiUrl);

                    if (movieFromApi != null)
                    {
                        DateTime currentTime = DateTime.UtcNow;
                        movie = CreateMovieFromApiResult(movieFromApi, user, currentTime);

                        movie.Users.Add(user);
                        MovieDbContext.Movies.Add(movie);
                    }
                }
                MovieDbContext.SaveChanges();

                return ReloadCurrentUrl();
            }
            catch (Exception ex)
            {
                return ReloadCurrentUrl();
            }
        }

        private Movie CreateMovieFromApiResult(MovieInfo.Result apiResult, User user, DateTime createdTime)
        {
            if (apiResult.title == null)
            {
                apiResult.title = apiResult.name;
            }
            else if (apiResult.title == null && apiResult.name == null)
            {
                apiResult.title = apiResult.original_title;
            }

            return new Movie
            {
                MovieDbId = apiResult.id,
                Title = apiResult.title,
                Poster = apiResult.poster_path_url,
                Rating = Math.Round(apiResult.vote_average, 1),
                Created = createdTime,
                Users = new List<User> { user }
            };
        }
    }
}