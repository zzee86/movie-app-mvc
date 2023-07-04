using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using movie_app_mvc.Models;
using System.Web;
using Microsoft.EntityFrameworkCore;

namespace movie_app_mvc.Controllers
{
    public class HomeController : Controller
    {
        private string connectionString = "server=localhost;database=saved_movies;user=root;";
        private const int PageSize = 20;



        //public async Task<IActionResult> Index(string searchQuery, int page = 1)
        //{
        //    // Get the user ID of the logged-in user
        //    string email = User.Identity.Name; // Assuming the email is stored in the "Name" claim

        //    // Retrieve the user ID from the loginDetails table
        //    string testing = GetUserIdByEmail(email);

        //    if (string.IsNullOrEmpty(searchQuery))
        //    {
        //        return await LoadMovies(page, testing);
        //        return await LoadPopularMovies(page, testing);
        //    }
        //    else
        //    {
        //        return await SearchMovies(searchQuery, testing,  page);
        //    }
        //}

        public async Task<IActionResult> Index(string searchQuery, int page = 1)
        {
            // Get the user ID of the logged-in user
            string email = User.Identity.Name; // Assuming the email is stored in the "Name" claim

            // Retrieve the user ID from the loginDetails table
            string testing = GetUserIdByEmail(email);

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
            string apiUrl = $"https://api.themoviedb.org/3/trending/all/day?language=en-US&api_key=ca80dfbe1afe5a1a97e4401ff534c4e4&page={page}";
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
            string apiUrl = $"https://api.themoviedb.org/3/movie/popular?language=en-US&api_key=ca80dfbe1afe5a1a97e4401ff534c4e4&page={page}";
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
            string apiUrl = $"https://api.themoviedb.org/3/movie/top_rated?language=en-US&api_key=ca80dfbe1afe5a1a97e4401ff534c4e4&page={page}";
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


        //public async Task<IActionResult> LoadPopularMovies(int page, string userID)
        //{
        //    string apiUrl = $"https://api.themoviedb.org/3/movie/popular?language=en-US&api_key=ca80dfbe1afe5a1a97e4401ff534c4e4&page={page}";
        //    MovieInfo.Root movieInfo = await FetchMovies(apiUrl);

        //    if (movieInfo != null)
        //    {
        //        List<MovieInfo.Result> movieResults = ProcessMovieResults(movieInfo.results, userID);

        //        ViewBag.CurrentPage = page;
        //        ViewBag.TotalPages = movieInfo.total_pages;
        //        //ViewBag.TotalPages = (int)Math.Ceiling((double)movieInfo.total_results / PageSize);


        //        return View(movieResults);
        //    }

        //    return View("Index");
        //}

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

        //private List<MovieInfo.Result> ProcessMovieResults(List<MovieInfo.Result> results)
        //{
        //    List<MovieInfo.Result> movieResults = new List<MovieInfo.Result>();

        //    foreach (var movie in results)
        //    {
        //        if (string.IsNullOrEmpty(movie.title))
        //        {
        //            // Use the name property if title is empty
        //            movie.title = movie.name;
        //        }

        //        if (movie.known_for != null && movie.known_for.Count > 0)
        //        {
        //            movie.title = movie.known_for[0].title;
        //            movie.poster_path = movie.known_for[0].poster_path;
        //        }

        //        //if (string.IsNullOrEmpty(movie.poster_path) && movie.known_for != null && movie.known_for.Count > 0)
        //        //{
        //        //    movie.poster_path = movie.known_for[0].poster_path;
        //        //}

        //        movie.IsSaved = MovieIsSaved(movie.title);

        //        movieResults.Add(movie);
        //    }

        //    return movieResults;
        //}
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

                movie.IsSaved = MovieIsSaved(movie.title, userID);

                movieResults.Add(movie);
            }

            return movieResults;
        }







        private async Task<List<MovieInfo.Result>> SearchMovies(string searchQuery, string userID, int pageNumber = 1)
        {
            string url = $"https://api.themoviedb.org/3/search/multi?language=en-US&api_key=ca80dfbe1afe5a1a97e4401ff534c4e4&query={searchQuery}&page={pageNumber}";
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


        public async Task<IActionResult> SaveMovie(string title, string overview, string poster, double rating, string searchQuery, string name, int page = 1)
        {
            // Get the user ID of the logged-in user
            string email = User.Identity.Name; // Assuming the email is stored in the "Name" claim

            // Retrieve the user ID from the loginDetails table
            string userId = GetUserIdByEmail(email);
            if (userId == null)
            {
                // User ID not found
                // Handle the error or redirect as needed
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(overview))
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "INSERT INTO savedMovies (title, overview, poster, dateTimeInsertion, userId, rating) VALUES (@Title, @Overview, @Poster, NOW(), @UserId, @Rating)";

                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Title", title);
                    command.Parameters.AddWithValue("@Overview", overview);
                    command.Parameters.AddWithValue("@Poster", poster);
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@Rating", rating);
                    command.ExecuteNonQuery();
                }
            }

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

        private string GetUserIdByEmail(string email)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT userID FROM loginDetails WHERE email = @Email";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Email", email);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        // Return the user ID if found
                        return reader.GetString("userID");
                    }
                }
            }

            // Return null if user ID not found
            return null;
        }




        public IActionResult SavedMovies(string title, int page = 1)
        {
            string email = User.Identity.Name; // Assuming the email is stored in the "Name" claim

            string userId = GetUserIdByEmail(email);
            if (userId == null)
            {
                // User ID not found
                // Handle the error or redirect as needed
                return RedirectToAction("Index");
            }

            int pageSize = 28;
            List<SavedMovie> savedMovies = GetMoviesFromDatabase(title, userId, page, pageSize);
            ViewBag.SavedPage = page;
            ViewBag.SearchQuery = title;

            int totalCount = GetTotalMovieCount(title, userId);

            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalPages = totalPages;

            bool hasNextPage = (page * pageSize) < totalCount;

            ViewBag.HasNextPage = hasNextPage;
            ViewBag.NextPage = page + 1;
            ViewBag.PreviousPage = page - 1;

            return View(savedMovies);
        }


        private int GetTotalMovieCount(string title, string userId)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = string.IsNullOrEmpty(title)
                    ? "SELECT COUNT(*) FROM savedMovies WHERE userId = @UserId"
                    : "SELECT COUNT(*) FROM savedMovies WHERE userId = @UserId AND title LIKE @Title";

                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);
                if (!string.IsNullOrEmpty(title))
                {
                    command.Parameters.AddWithValue("@Title", $"%{title}%");
                }

                return Convert.ToInt32(command.ExecuteScalar());
            }
        }





        public IActionResult RemoveMovie(string title, string searchQuery, int page = 1)
        {
            if (!string.IsNullOrEmpty(title))
            {
                // Remove the movie from the database
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "DELETE FROM savedMovies WHERE title = @Title";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Title", title);
                    command.ExecuteNonQuery();
                }
            }

            // Determine the referring URL to refresh the current page
            string referringUrl = Request.Headers["Referer"].ToString();
            if (string.IsNullOrEmpty(referringUrl))
            {
                // If referring URL is empty, redirect to the home page
                return RedirectToAction("Index");
            }
            else
            {
                // Redirect back to the referring URL
                return Redirect(referringUrl);
            }
        }

        private List<SavedMovie> GetMoviesFromDatabase(string title, string userId, int page, int pageSize)
        {
            List<SavedMovie> movies = new List<SavedMovie>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = string.IsNullOrEmpty(title)
                    ? "SELECT * FROM savedMovies WHERE userId = @UserId"
                    : "SELECT * FROM savedMovies WHERE userId = @UserId AND title LIKE @Title";

                query += $" ORDER BY dateTimeInsertion DESC LIMIT {pageSize} OFFSET {(page - 1) * pageSize}";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);
                if (!string.IsNullOrEmpty(title))
                {
                    command.Parameters.AddWithValue("@Title", $"%{title}%");
                }

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SavedMovie movie = new SavedMovie
                        {
                            Id = reader.GetInt32("id"),
                            Title = reader.GetString("title"),
                            Overview = reader.GetString("overview"),
                            Poster = reader.GetString("poster"),
                            Rating = reader.GetDouble("rating")
                        };

                        movies.Add(movie);
                    }
                }
            }

            return movies;
        }




        private bool MovieIsSaved(string title, string userID)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM savedMovies WHERE title = @title AND userID = @userID";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@title", title);
                command.Parameters.AddWithValue("@userID", userID);

                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }





        public async Task<ActionResult> MovieDetails(string title)
        {
            string apiUrl = $"https://api.themoviedb.org/3/search/multi?language=en-US&api_key=ca80dfbe1afe5a1a97e4401ff534c4e4&query={title}";
            MovieInfo.Root movieDetails;

            movieDetails = await FetchMovies(apiUrl);

            MovieInfo.Result movie = movieDetails.results.FirstOrDefault(); // Get the first movie from the results (or null if no items)

            if (movie == null)
            {
                // Handle the case when no movie is found with the given title
                return RedirectToAction("Index", "Home");
            }

            return View(movie);
        }




        public ActionResult SavedMovieDetails(string title)
        {
            List<SavedMovie> movies = new List<SavedMovie>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT * FROM savedMovies WHERE title = @Title";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Title", title);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SavedMovie movie = new SavedMovie
                        {
                            Id = reader.GetInt32("id"),
                            Title = reader.GetString("title"),
                            Overview = reader.GetString("overview"),
                            Poster = reader.GetString("poster"),
                            Rating = reader.GetDouble("rating")
                        };

                        movies.Add(movie);
                    }
                }
            }

            SavedMovie selectedMovie = movies.FirstOrDefault();

            if (selectedMovie == null)
            {
                // Handle the case when no movie is found with the given title
                return RedirectToAction("Index", "Home");
            }

            return View(selectedMovie);
        }

    }
}
