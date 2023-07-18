using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using movie_app_mvc.Models;
using ColorThiefDotNet;
using System.Drawing;
using System.Net;

namespace movie_app_mvc.Controllers
{
    public class DetailsController : Controller
    {
        private string connectionString = "server=localhost;database=saved_movies;user=root;";

        public IActionResult Index(string title, int id)
        {
            return RedirectToAction("MovieDetails", new { title = title, id = id });

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

                //movie.IsSaved = MovieIsSaved(movie.title, userID);

                movieResults.Add(movie);
            }

            return movieResults;
        }

        public async Task<ActionResult> MovieDetails(string title, int id)
        {
            ViewBag.TestingTitle = title;
            ViewBag.TestingID = id;

            // Get details on the movie
            string apiUrl = $"https://api.themoviedb.org/3/search/multi?language=en-US&api_key=ca80dfbe1afe5a1a97e4401ff534c4e4&query={title}";
            MovieInfo.Root movieDetails = await FetchMovies(apiUrl);
            MovieInfo.Result movie = movieDetails.results.FirstOrDefault();

            if (movie == null)
            {
                // Handle the case when no movie is found with the given title
                return RedirectToAction("Index", "Home");
            }

            string media_type = (movie.media_type == "movie") ? "movie" : "tv";
            // Get trailer
            VideoInfo.Result video = await GetTrailer(media_type, id);

            // Main content on details page
            string movie_tv_url = (media_type == "tv") ? $"https://api.themoviedb.org/3/{media_type}/{id}?api_key=ca80dfbe1afe5a1a97e4401ff534c4e4" : $"https://api.themoviedb.org/3/{media_type}/{id}?api_key=ca80dfbe1afe5a1a97e4401ff534c4e4";
            TvShowDetails.Root movie_tv_details = await FetchTvShows(movie_tv_url);

            string media_pg_rating_url = (media_type == "tv") ? $"https://api.themoviedb.org/3/{media_type}/{id}/content_ratings?api_key=ca80dfbe1afe5a1a97e4401ff534c4e4" : $"https://api.themoviedb.org/3/{media_type}/{id}/release_dates?api_key=ca80dfbe1afe5a1a97e4401ff534c4e4";

            if (media_type == "tv")
            {
                TvPGRating.Root media_pg_rating = await FetchTvShowPGRating(media_pg_rating_url);

                if (media_pg_rating != null)
                {
                    var result = media_pg_rating.results.FirstOrDefault(r => r.iso_3166_1 == "US");

                    ViewBag.TvPGRating = (result != null) ? result.rating : media_pg_rating.results[0].rating;
                    ViewBag.Media_Overview = movie_tv_details.overview;
                }
            }
            else
            {
                MoviePGRating.Root media_pg_rating = await FetchMoviePGRating(media_pg_rating_url);

                if (media_pg_rating != null)
                {
                    var result = media_pg_rating.results.FirstOrDefault(r => r.iso_3166_1 == "GB");

                    if (result != null)
                    {
                        ViewBag.MoviePGRating = (result != null) ? result.release_dates.FirstOrDefault()?.certification : result.release_dates[0].certification;
                    }
                }
                ViewBag.Media_Overview = movie.overview;
            }

            if (movie == null || movie.poster_path == null)
            {
                // Redirect the user to another action or view
                return RedirectToAction("Index", "Home");
            }

            if (movie_tv_details != null)
            {
                if (media_type == "tv")
                {
                    TvShowDetails.LastEpisodeToAir season = movie_tv_details.last_episode_to_air;
                    ViewBag.EpisodeRuntime = season?.runtime ?? 24;

                    var seasonInfo = movie_tv_details.seasons.OrderByDescending(s => s.season_number).FirstOrDefault();
                    int episodeCount = seasonInfo?.episode_count ?? 0;
                    int seasonCount = seasonInfo?.season_number ?? 0;

                    int episodeTotalCount = movie_tv_details.seasons.Sum(s => s.episode_count);
                    ViewBag.episodeTotalCount = (episodeTotalCount != null) ? episodeTotalCount : 0;
                    ViewBag.SeasonNumber = (seasonCount != null) ? seasonCount : 1;
                    ViewBag.SeasonEpisodeCount = (episodeCount != null) ? episodeCount : 0;
                    var seasonrelease = (seasonInfo != null) ? seasonInfo?.air_date : "00:00:0000";
                    ViewBag.SeasonRuntime = (seasonInfo != null) ? season?.runtime : 0;

                    DateTime releaseDate = DateTime.Parse(seasonInfo?.air_date);
                    ViewBag.ReleaseDate = releaseDate.Year.ToString();
                    ViewBag.FullReleaseDate = releaseDate;

                    DateTime initialRelease = DateTime.Parse(movie_tv_details.first_air_date);
                    ViewBag.InitialReleaseDate = initialRelease.Year.ToString();

                    DateTime finalRelease = DateTime.Parse(movie_tv_details.last_air_date);
                    ViewBag.FinalReleaseDate = finalRelease.Year.ToString();

                    if (movie_tv_details.status == "Ended" && movie_tv_details.seasons.Sum(s => s.season_number) >= 2)
                    {
                        ViewBag.SeriesEnded = "yes";
                    }
                    else
                    {
                        ViewBag.SeriesEnded = "no";
                    }
                }
                else
                {
                    DateTime releaseDate = DateTime.Parse(movie_tv_details.release_date);
                    ViewBag.ReleaseDate = releaseDate.Year.ToString();
                    ViewBag.FullReleaseDate = releaseDate.ToString("dd/MM/yyyy");

                    ConvertRuntime(movie_tv_details.runtime);
                }

                // Common between the media types
                List<string> genres = movie_tv_details.genres.Select(genre => genre.name).ToList();
                ViewBag.Genres = genres;

                ViewBag.CallMethod = "SaveMoviePosterToDatabase method called";
                string posterUrl = movie.poster_path_url; // Replace this with the actual property containing the poster URL
                SaveMoviePosterToDatabase(posterUrl);
            }

            // Get details on the cast
            List<Actors.Cast> castInfo = await GetCastInfo(media_type, id);
            ViewBag.CastList = castInfo;

            ViewBag.MovieDetailsTitle = title;
            ViewBag.MovieDetailsID = id;
            ViewBag.Backup_Title = title;
            ViewBag.Media_Key = media_type;
            ViewBag.movieID = id;
            ViewBag.MovieKey = (video != null) ? video.key : "unavailable";

            return View("MovieDetails", movie);
        }

        private async Task<VideoInfo.Result> GetTrailer(string mediaType, int id)
        {
            string apiUrl = $"https://api.themoviedb.org/3/{mediaType}/{id}/videos?api_key=ca80dfbe1afe5a1a97e4401ff534c4e4";
            VideoInfo.Root videoDetails = await FetchVideos(apiUrl);
            VideoInfo.Result video = videoDetails.results.FirstOrDefault(v => v.type.Equals("Trailer", StringComparison.OrdinalIgnoreCase));

            return video;
        }

        private async Task<List<Actors.Cast>> GetCastInfo(string mediaType, int id)
        {
            string castApi = $"https://api.themoviedb.org/3/{mediaType}/{id}/credits?language=en-US&api_key=ca80dfbe1afe5a1a97e4401ff534c4e4";
            Actors.Root castDetails = await FetchActors(castApi);
            List<Actors.Cast> castInfo = castDetails.cast.Take(10).ToList();

            return castInfo;
        }

        private void ConvertRuntime(int runtime)
        {
            int hours = runtime / 60;
            int minutes = runtime % 60;
            string formattedRuntime = $"{hours}h {minutes}m";
            ViewBag.Runtime = formattedRuntime;
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

                        // Set the dominant color in the ViewBag
                        ViewBag.DominantColorRed = pixel.R;
                        ViewBag.DominantColorGreen = pixel.G;
                        ViewBag.DominantColorBlue = pixel.B;

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

        private async Task<Actors.Root> FetchActors(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine(json); // Print the JSON response to the console
                var info = JsonConvert.DeserializeObject<Actors.Root>(json);

                return info;
            }
        }

        private async Task<TvPGRating.Root> FetchTvShowPGRating(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine(json); // Print the JSON response to the console
                var info = JsonConvert.DeserializeObject<TvPGRating.Root>(json);

                return info;
            }
        }

        private async Task<MoviePGRating.Root> FetchMoviePGRating(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine(json); // Print the JSON response to the console
                var info = JsonConvert.DeserializeObject<MoviePGRating.Root>(json);

                return info;
            }
        }
    }
}
