using System.Diagnostics;
using movie_app_mvc.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using movie_app_mvc.Models;
using MySql.Data.MySqlClient;

namespace movie_app_mvc.Controllers;

public class HomeController : Controller
{

    int currentPage = 1;
    int totalPages = 5;
    int pageSize = 20;
    private string connectionString = "server=localhost;database=saved_movies;user=root;";


    // Task<IActionResult> when communicating with db or web requests
    public async Task<IActionResult> Index(string searchQuery, int page = 1)
    {
        currentPage = page;

        if (string.IsNullOrEmpty(searchQuery))
        {
            return await LoadMovies(currentPage);
        }
        else
        {
            return await SearchMovies(searchQuery, page);
        }
    }

    public async Task<ActionResult> LoadMovies(int page)
    {
        string apiUrl = $"https://api.themoviedb.org/3/trending/all/day?language=en-US&api_key=ca80dfbe1afe5a1a97e4401ff534c4e4&page={page}";
        MovieInfo.Root movieInfo = await FetchMovies(apiUrl);

        if (movieInfo != null)
        {
            List<MovieInfo.Result> movieResults = ProcessMovieResults(movieInfo.results);

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

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

            // Pass the current page, total pages, and search query to the view
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = totalPages;
            ViewBag.SearchQuery = searchQuery;

            return View(movieResults);
        }
        return View("Index");

    }

    public async Task<IActionResult> SaveMovie(string title, string overview, string poster, string searchQuery, int page = 1)
    {
        if (!string.IsNullOrEmpty(title) && !string.IsNullOrEmpty(overview))
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "INSERT INTO savedMovies (Title, Overview, Poster) VALUES (@Title, @Overview, @Poster)";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Title", title);
                command.Parameters.AddWithValue("@Overview", overview);
                command.Parameters.AddWithValue("@Poster", poster);
                command.ExecuteNonQuery();
            }

            // Redirect back to the current page with the same search query and page number
            return RedirectToAction("Index", new { searchQuery, page });
        }

        return View();
    }


    public IActionResult SavedMovies()
    {
        List<SavedMovie> savedMovies;

        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            string query = "SELECT * FROM savedMovies";
            MySqlCommand command = new MySqlCommand(query, connection);
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                savedMovies = new List<SavedMovie>();

                while (reader.Read())
                {
                    int id = reader.GetInt32("Id");
                    string title = reader.GetString("Title");
                    string overview = reader.GetString("Overview");
                    string poster = reader.GetString("Poster");

                    SavedMovie savedMovie = new SavedMovie
                    {
                        Id = id,
                        Title = title,
                        Overview = overview,
                        Poster = poster
                    };

                    savedMovies.Add(savedMovie);
                }
            }
        }

        return View(savedMovies);
    }


    private bool MovieIsSaved(string title)
    {
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            string query = "SELECT COUNT(*) FROM savedMovies WHERE Title = @Title";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Title", title);

            int count = Convert.ToInt32(command.ExecuteScalar());
            return count > 0;
        }
    }


    public async Task<IActionResult> RemoveMovie(string title, string searchQuery, int page = 1)
    {
        if (!string.IsNullOrEmpty(title))
        {
            // Remove the movie from the database
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "DELETE FROM savedMovies WHERE Title = @Title";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Title", title);
                command.ExecuteNonQuery();
            }

            // Build the URL for the savedMovies action with the search query and page number
            var url = Url.Action("savedMovies", "Home", new { searchQuery, page });

            // Redirect to the savedMovies action with the search query and page number
            return Redirect(url);
        }

        return View();
    }


}