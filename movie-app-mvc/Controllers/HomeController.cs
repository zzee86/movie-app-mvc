using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using movie_app_mvc.Models;

namespace movie_app_mvc.Controllers
{
    public class HomeController : Controller
    {
        private string connectionString = "server=localhost;database=saved_movies;user=root;";

        public async Task<IActionResult> Index(string searchQuery, int page = 1)
        {
            if (string.IsNullOrEmpty(searchQuery))
            {
                return await LoadMovies(page);
            }
            else
            {
                return await SearchMovies(searchQuery, page);
            }
        }

        public async Task<IActionResult> LoadMovies(int page)
        {
            string apiUrl = $"https://api.themoviedb.org/3/trending/all/day?language=en-US&api_key=ca80dfbe1afe5a1a97e4401ff534c4e4&page={page}";
            MovieInfo.Root movieInfo = await FetchMovies(apiUrl);

            if (movieInfo != null)
            {
                List<MovieInfo.Result> movieResults = ProcessMovieResults(movieInfo.results);

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = movieInfo.total_pages;

                return View(movieResults);
            }

            return View("Index");
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

        private List<MovieInfo.Result> ProcessMovieResults(List<MovieInfo.Result> results)
        {
            List<MovieInfo.Result> movieResults = new List<MovieInfo.Result>();

            foreach (var movie in results)
            {
                if (string.IsNullOrEmpty(movie.title))
                {
                    // Use the name property if title is empty
                    movie.title = movie.name;
                }

                movie.IsSaved = MovieIsSaved(movie.title);

                movieResults.Add(movie);
            }

            return movieResults;
        }

        private async Task<IActionResult> SearchMovies(string searchQuery, int pageNumber = 1)
        {
            string url = $"https://api.themoviedb.org/3/search/multi?language=en-US&api_key=ca80dfbe1afe5a1a97e4401ff534c4e4&query={searchQuery}&page={pageNumber}";
            MovieInfo.Root movieInfo = await FetchMovies(url);

            if (movieInfo != null)
            {
                List<MovieInfo.Result> movieResults = ProcessMovieResults(movieInfo.results);

                ViewBag.CurrentPage = pageNumber;
                ViewBag.TotalPages = movieInfo.total_pages;
                ViewBag.SearchQuery = searchQuery;

                return View(movieResults);
            }

            return View("Index");
        }

        public async Task<IActionResult> SaveMovie(string title, string overview, string poster, string searchQuery, string name, int page = 1)
        {
            if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(overview))
            {
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "INSERT INTO savedMovies (title, overview, poster) VALUES (@Title, @Overview, @Poster)";
                    MySqlCommand command = new MySqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Title", title);
                    command.Parameters.AddWithValue("@Overview", overview);
                    command.Parameters.AddWithValue("@Poster", poster);
                    command.ExecuteNonQuery();
                }

                // Redirect back to the current page with the same search query and page number
                return RedirectToAction("Index", new { searchQuery, page });
            }

            List<SavedMovie> movies = GetMoviesFromDatabase(name);
            return View();
        }

        public IActionResult SavedMovies(string title)
        {
            List<SavedMovie> savedMovies = GetMoviesFromDatabase(title);
            return View(savedMovies);
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

        private List<SavedMovie> GetMoviesFromDatabase(string title)
        {
            List<SavedMovie> movies = new List<SavedMovie>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = string.IsNullOrEmpty(title) ? "SELECT * FROM savedMovies" : $"SELECT * FROM savedMovies WHERE title LIKE '%{title}%'";
                MySqlCommand command = new MySqlCommand(query, connection);

                using (MySqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SavedMovie movie = new SavedMovie
                        {
                            Id = reader.GetInt32("id"),
                            Title = reader.GetString("title"),
                            Overview = reader.GetString("overview"),
                            Poster = reader.GetString("poster")
                        };

                        movies.Add(movie);
                    }
                }
            }

            return movies;
        }

        private bool MovieIsSaved(string title)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM savedMovies WHERE title = @Title";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Title", title);

                int count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }
    }
}
