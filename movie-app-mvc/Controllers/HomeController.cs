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
        private const int PageSize = 20;



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
                //ViewBag.TotalPages = (int)Math.Ceiling((double)movieInfo.total_results / PageSize);


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
                return RedirectToAction("SavedMovies", new { title = searchQuery, page });
            }

            List<SavedMovie> movies = GetMoviesFromDatabase(searchQuery, page, 32); // Pass the required parameters
            return View(movies); // Pass the retrieved movies to the view
        }



        public IActionResult SavedMovies(string title, int page = 1)
        {
            int pageSize = 32;
            List<SavedMovie> savedMovies = GetMoviesFromDatabase(title, page, pageSize);
            ViewBag.SavedPage = page;
            ViewBag.SearchQuery = title;

            // Get the total count of movies for pagination
            int totalCount = GetTotalMovieCount(title);

            int totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalPages = totalPages;

            // Calculate if there are more movies beyond the current page
            bool hasNextPage = (page * pageSize) < totalCount;

            // Set ViewBag variables for pagination
            ViewBag.HasNextPage = hasNextPage;
            ViewBag.NextPage = page + 1;
            ViewBag.PreviousPage = page - 1;

            return View(savedMovies);
        }

        private int GetTotalMovieCount(string title)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = string.IsNullOrEmpty(title) ? "SELECT COUNT(*) FROM savedMovies" : $"SELECT COUNT(*) FROM savedMovies WHERE title LIKE '%{title}%'";
                MySqlCommand command = new MySqlCommand(query, connection);

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

        private List<SavedMovie> GetMoviesFromDatabase(string title, int page, int pageSize)
        {
            List<SavedMovie> movies = new List<SavedMovie>();

            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = string.IsNullOrEmpty(title) ? "SELECT * FROM savedMovies" : $"SELECT * FROM savedMovies WHERE title LIKE '%{title}%'";
                query += $" LIMIT {pageSize} OFFSET {(page - 1) * pageSize}";
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
