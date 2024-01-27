using Microsoft.AspNetCore.Mvc;
//using MySql.Data.MySqlClient;
using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Threading.Tasks;
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
using MovieApp.Services.APIModels;
using MovieApp.Services.Services;
using MovieApp.Services.Interfaces;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Routing;
//using movie_app_mvc.Models.Users;
//using Microsoft.EntityFrameworkCore.Internal;

namespace movie_app_mvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly IMovieDbContext MovieDbContext;
        private readonly IMovieService MovieService;
        private readonly IHttpContextAccessor HttpContextAccessor;

        public HomeController(IMovieDbContext movieDbContext, IMovieService movieService, IHttpContextAccessor httpContextAccessor)
        {
            this.MovieDbContext = movieDbContext;
            this.MovieService = movieService;
            this.HttpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> Index(string searchQuery, int page = 1)
        {
            // Get the user ID of the logged-in user
            // Assuming the email is stored in the "Name" claim
            // string email = User.Identity.Name; 
            // var testing = "testing";
            // Retrieve the user ID from the loginDetails table
            TempData["CurrentDateTime"] = DateTime.Now.ToString();

            List<MovieInfo.Result> trendingMovies = new List<MovieInfo.Result>();
            List<MovieInfo.Result> popularMovies = new List<MovieInfo.Result>();
            List<MovieInfo.Result> topRatedMovies = new List<MovieInfo.Result>();

            if (string.IsNullOrEmpty(searchQuery))
            {
                trendingMovies = await LoadMovies(page);
                popularMovies = await LoadPopularMovies(page);
                topRatedMovies = await LoadTopRatedMovies(page);
            }
            else
            {
                trendingMovies = await SearchMovies(searchQuery, page);
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


        public async Task<List<MovieInfo.Result>> LoadMovies(int page)
        {
            string apiUrl = $"https://api.themoviedb.org/3/trending/all/day?language=en-US&api_key={Constants.Constants.apiKey}&page={page}";
            MovieInfo.Root movieInfo = await MovieService.FetchMovies(apiUrl);

            if (movieInfo != null)
            {
                List<MovieInfo.Result> movieResults = MovieService.ProcessMovieResults(movieInfo.results, User.Identity.IsAuthenticated);

                if (movieInfo.results.Count() >= 1)
                {
                    TempData["ProcessMovieCount"] = "Active";
                }

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = movieInfo.total_pages;
                //ViewBag.TotalPages = (int)Math.Ceiling((double)movieInfo.total_results / PageSize);

                return movieResults;
            }

            return null;
        }

        public async Task<List<MovieInfo.Result>> LoadPopularMovies(int page)
        {
            string apiUrl = $"https://api.themoviedb.org/3/movie/popular?language=en-US&api_key={Constants.Constants.apiKey}&page={page}";

            MovieInfo.Root movieInfo = await MovieService.FetchMovies(apiUrl);

            if (movieInfo != null)
            {
                List<MovieInfo.Result> movieResults = MovieService.ProcessMovieResults(movieInfo.results, User.Identity.IsAuthenticated);

                if (movieInfo.results.Count() >= 1)
                {
                    TempData["ProcessMovieCount"] = "Active";
                }

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = movieInfo.total_pages;
                //ViewBag.TotalPages = (int)Math.Ceiling((double)movieInfo.total_results / PageSize);

                return movieResults;
            }

            return null;
        }

        public async Task<List<MovieInfo.Result>> LoadTopRatedMovies(int page)
        {
            string apiUrl = $"https://api.themoviedb.org/3/movie/top_rated?language=en-US&api_key={Constants.Constants.apiKey}&page={page}";
            MovieInfo.Root movieInfo = await MovieService.FetchMovies(apiUrl);

            if (movieInfo != null)
            {
                List<MovieInfo.Result> movieResults = MovieService.ProcessMovieResults(movieInfo.results, User.Identity.IsAuthenticated);

                if (movieInfo.results.Count() >= 1)
                {
                    TempData["ProcessMovieCount"] = "Active";
                }

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = movieInfo.total_pages;
                //ViewBag.TotalPages = (int)Math.Ceiling((double)movieInfo.total_results / PageSize);

                return movieResults;
            }

            return null;
        }


        private async Task<List<MovieInfo.Result>> SearchMovies(string searchQuery, int pageNumber = 1)
        {
            string url = $"https://api.themoviedb.org/3/search/multi?language=en-US&api_key={Constants.Constants.apiKey}&query={searchQuery}&page={pageNumber}";
            MovieInfo.Root movieInfo = await MovieService.FetchMovies(url);

            if (movieInfo != null)
            {
                List<MovieInfo.Result> movieResults = MovieService.ProcessMovieResults(movieInfo.results, User.Identity.IsAuthenticated);


                if (movieInfo.results.Count() >= 1)
                {
                    TempData["ProcessMovieCount"] = "Active";
                }

                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = movieInfo.total_pages;
                ViewBag.SearchQuery = searchQuery;

                return movieResults;
            }

            return null;
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
            return MovieService.ReloadCurrentUrl();
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
                MovieInfo.Root movieExtra = await MovieService.FetchMovies(apiUrlExtra);
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
                    MovieInfo.Result movieFromApi = await MovieService.FetchMovie(apiUrl);

                    if (movieFromApi != null)
                    {
                        DateTime currentTime = DateTime.UtcNow;
                        movie = CreateMovieFromApiResult(movieFromApi, user, currentTime);

                        movie.Users.Add(user);
                        MovieDbContext.Movies.Add(movie);
                    }
                }
                MovieDbContext.SaveChanges();

                return MovieService.ReloadCurrentUrl();
            }
            catch (Exception ex)
            {
                return MovieService.ReloadCurrentUrl();
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