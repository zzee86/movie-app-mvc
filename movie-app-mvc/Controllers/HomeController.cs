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

    [HttpPost]
    public async Task<IActionResult> SaveMovie(string title, string overview, string searchQuery, int page = 1)
    {
        // Save the movie to the database
        using (MySqlConnection connection = new MySqlConnection(connectionString))
        {
            connection.Open();

            string query = "INSERT INTO savedMovies (Title, Overview) VALUES (@Title, @Overview)";
            MySqlCommand command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Title", title);
            command.Parameters.AddWithValue("@Overview", overview);
            command.ExecuteNonQuery();
        }

        // Redirect back to the current page with the same query and page number
        return RedirectToAction("Index", new { searchQuery, page });
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

                    SavedMovie savedMovie = new SavedMovie
                    {
                        Id = id,
                        Title = title,
                        Overview = overview
                    };

                    savedMovies.Add(savedMovie);
                }
            }
        }

        return View(savedMovies);
    }
}