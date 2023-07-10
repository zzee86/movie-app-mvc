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
using static movie_app_mvc.Models.TvShowDetails;
using System.Net;
using ColorThiefDotNet;
using System.Drawing;


namespace movie_app_mvc.Controllers
{
    public class HomeController : Controller
    {
        private string connectionString = "server=localhost;database=saved_movies;user=root;";
        private const int PageSize = 20;


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

            ClearTmpMoviePosterTable();

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





        public async Task<ActionResult> MovieDetails(string title, int id)
        {
            // Get details on the movie
            string apiUrl = $"https://api.themoviedb.org/3/search/multi?language=en-US&api_key=ca80dfbe1afe5a1a97e4401ff534c4e4&query={title}";
            MovieInfo.Root movieDetails = await FetchMovies(apiUrl);
            MovieInfo.Result movie = movieDetails.results.FirstOrDefault();

            if (movie == null)
            {
                // Handle the case when no movie is found with the given title
                return RedirectToAction("Index", "Home");
            }

            // Get trailer
            string media_type = (movie.media_type == "movie") ? "movie" : "tv";
            string apiUrl2 = $"https://api.themoviedb.org/3/{media_type}/{id}/videos?api_key=ca80dfbe1afe5a1a97e4401ff534c4e4";
            VideoInfo.Root videoDetails = await FetchVideos(apiUrl2);
            VideoInfo.Result video = videoDetails.results.FirstOrDefault(v => v.type.Equals("Trailer", StringComparison.OrdinalIgnoreCase));


            // Main content on details page
            string movie_tv_url = (media_type == "tv") ? $"https://api.themoviedb.org/3/{media_type}/{id}?api_key=ca80dfbe1afe5a1a97e4401ff534c4e4" : $"https://api.themoviedb.org/3/{media_type}/{id}?api_key=ca80dfbe1afe5a1a97e4401ff534c4e4"; ;

            TvShowDetails.Root movie_tv_details = await FetchTvShows(movie_tv_url);

            if (movie_tv_details != null)
            {
                if (media_type == "tv")
                {
                    TvShowDetails.LastEpisodeToAir season = movie_tv_details.last_episode_to_air;
                    ViewBag.EpisodeRuntime = (season != null) ? season.runtime.Value : 24;

                    var seasonInfo = movie_tv_details.seasons.OrderByDescending(s => s.season_number).FirstOrDefault();
                    int episodeCount = seasonInfo?.episode_count ?? 0;
                    int seasonCount = seasonInfo?.season_number ?? 0;


                    int episodeTotalCount = movie_tv_details.seasons.Sum(s => s.episode_count);
                    ViewBag.episodeTotalCount = (episodeTotalCount != null) ? episodeTotalCount : 0;
                    ViewBag.SeasonNumber = (seasonCount != null) ? seasonCount : 1;
                    ViewBag.SeasonEpisodeCount = (episodeCount != null) ? episodeCount : 0;
                    ViewBag.SeasonAirDate = (seasonInfo != null) ? seasonInfo?.air_date : "00:00:0000";
                    ViewBag.SeasonRuntime = (seasonInfo != null) ? season?.runtime : 0;
                }
                else
                {

                    ViewBag.ReleaseDate = movie_tv_details.release_date;
                    ViewBag.Runtime = movie_tv_details.runtime;

                }

                // Common between the media types
                List<string> genres = movie_tv_details.genres.Select(genre => genre.name).ToList();

                ViewBag.Genres = genres;


                ViewBag.CallMethod = "SaveMoviePosterToDatabase method called";
                string posterUrl = movie.poster_path_url; // Replace this with the actual property containing the poster URL
                SaveMoviePosterToDatabase(posterUrl);
            }

            // Developer use
            ViewBag.ReleaseDate = movie.release_date;
            ViewBag.Genre = movie.genre_ids;

            ViewBag.Backup_Title = title;
            ViewBag.Media_Key = media_type;
            ViewBag.movieID = id;
            ViewBag.MovieKey = (video != null) ? video.key : "unavailable";

            return View("~/Views/Home/MovieDetails.cshtml", movie);
        }



        private void SaveMoviePosterToDatabase(string posterUrl)
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Insert the poster URL into the database
                string query = "INSERT INTO tmpMoviePoster (poster) VALUES (@Poster)";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.Parameters.AddWithValue("@Poster", posterUrl);
                command.ExecuteNonQuery();

                // Get the inserted row's ID
                long insertedId = command.LastInsertedId;

                // Download the poster image from the URL
                using (var webClient = new WebClient())
                {
                    byte[] imageData = webClient.DownloadData(posterUrl);
                    using (var image = SixLabors.ImageSharp.Image.Load<Rgba32>(imageData))
                    {
                        // Extract the dominant color from the poster image
                        var pixel = image[0, 0];
                        string dominantColor = $"#{pixel.R:X2}{pixel.G:X2}{pixel.B:X2}";

                        // Update the dominant color in the database
                        string updateQuery = "UPDATE tmpMoviePoster SET dominantColor = @DominantColor WHERE id = @Id";
                        MySqlCommand updateCommand = new MySqlCommand(updateQuery, connection);
                        updateCommand.Parameters.AddWithValue("@DominantColor", dominantColor);
                        updateCommand.Parameters.AddWithValue("@Id", insertedId);
                        updateCommand.ExecuteNonQuery();

                        // Store the dominant color in the ViewBag
                        ViewBag.DominantColor = dominantColor;

                        AddTransparency(dominantColor);
                    }
                }
            }
        }

        private void AddTransparency(string dominantColor)
        {

            double transparency = 0.7;

            int red = int.Parse(dominantColor.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);
            int green = int.Parse(dominantColor.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            int blue = int.Parse(dominantColor.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);

            // Combine the color components and transparency value
            string backgroundColor = $"rgba({red}, {green}, {blue}, {transparency})";

            ViewBag.DominantColorTransparency = backgroundColor;
        }

        private void ClearTmpMoviePosterTable()
        {
            using (MySqlConnection connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                string query = "DELETE FROM tmpMoviePoster";
                MySqlCommand command = new MySqlCommand(query, connection);
                command.ExecuteNonQuery();
            }
        }

        private async Task<VideoInfo.Root> FetchVideos(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var info = JsonConvert.DeserializeObject<VideoInfo.Root>(json);

                return info;
            }
        }

        private async Task<TvShowDetails.Root> FetchTvShows(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine(json); // Print the JSON response to the console
                var info = JsonConvert.DeserializeObject<TvShowDetails.Root>(json);

                return info;
            }
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
