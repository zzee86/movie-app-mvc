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
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using movie_app_data.Models;

namespace movie_app_mvc.Controllers
{
    public class DetailsController : Controller
    {
        private string connectionString = "server=localhost;database=saved_movies;user=root;";

        public async Task<ActionResult> MovieDetails(string title, int id)
        {
            // Try Catch to avoid crashing the app
            try
            {
                // Null-conditional operator to avoid crashing
                string email = User.Identity?.Name;

                string userID = email;

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
                ViewBag.MovieKey = (video != null) ? video.key : "unavailable";

                // Main content on details page
                string movie_tv_url = $"https://api.themoviedb.org/3/{media_type}/{id}?api_key=ca80dfbe1afe5a1a97e4401ff534c4e4";
                TvShowDetails.Root movie_tv_details = await FetchTvShows(movie_tv_url);

                string media_pg_rating_url = (media_type == "tv") ? $"https://api.themoviedb.org/3/{media_type}/{id}/content_ratings?api_key=ca80dfbe1afe5a1a97e4401ff534c4e4" : $"https://api.themoviedb.org/3/{media_type}/{id}/release_dates?api_key=ca80dfbe1afe5a1a97e4401ff534c4e4";

                if (media_type == "tv")
                {
                    TvPGRating.Root media_pg_rating = await FetchTvShowPGRating(media_pg_rating_url);

                    if (media_pg_rating != null)
                    {
                        var result = media_pg_rating.results.FirstOrDefault(r => r.iso_3166_1 == "US");

                        ViewBag.Media_PG_Rating = (result != null) ? result.rating : media_pg_rating.results[0].rating;
                        TempData["Media_Overview"] = movie_tv_details.overview;
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
                            ViewBag.Media_PG_Rating = (result != null) ? result.release_dates.FirstOrDefault()?.certification : result.release_dates[0].certification;
                        }
                    }
                }

                if (movie == null || movie.poster_path == null)
                {
                    // Redirect the user to another action or view
                    return RedirectToAction("Index", "Home");
                }

                // For save button on details page overriden with GetMediaRecommendations method
                ProcessMovieResults(movieDetails.results, userID);

                await GetMediaRecommendations(media_type, id, userID);
                await GetMediaProviders(media_type, id);




                if (movie_tv_details != null)
                {
                    if (media_type == "tv")
                    {
                        TVShowViewBags(movie_tv_details);
                    }
                    else
                    {
                        MovieViewBags(movie_tv_details);
                    }

                    ExtractPosterColour(movie_tv_details.poster_path_url);

                    // Common between the media types
                    List<string> genres = movie_tv_details.genres.Select(genre => genre.name).ToList();
                    ViewBag.Genres = genres;

                    string posterUrl = movie.poster_path_url;
                    // SaveMoviePosterToDatabase(posterUrl);
                    TempData["Media_Overview"] = movie_tv_details.overview;

                }

                // Get details on the cast
                List<Actors.Cast> castInfo = await GetCastInfo(media_type, id);
                ViewBag.CastList = castInfo;


                // Custom Values
                if (media_type == "tv" && !string.IsNullOrEmpty(ViewBag.ReleaseDate))
                {
                    TempData["ReleaseTitle"] = "(" + @ViewBag.InitialReleaseDate + " - " + @ViewBag.FinalReleaseDate + ")";

                    if (@ViewBag.InitialReleaseDate == @ViewBag.FinalReleaseDate)
                    {
                        TempData["ReleaseTitle"] = "(" + @ViewBag.InitialReleaseDate + ")";
                    }
                }

                else if (media_type == "MovieDetailsTitle" && !string.IsNullOrEmpty(ViewBag.ReleaseDate))
                {
                    TempData["ReleaseTitle"] = "(" + @ViewBag.ReleaseDate + ")";
                }
                else
                {
                    TempData["ReleaseTitle"] = "(" + @ViewBag.ReleaseDate + ")";
                }


                TempData["MovieDetailsTitle"] = title;
                TempData["Media_Type"] = media_type;
                TempData["movieID"] = id;
                TempData["MovieKey"] = (video != null) ? video.key : "unavailable";

                return View("MovieDetails", movie);

            }
            catch (Exception ex)
            {
                TempData["DetailsError"] = true;

                string referringUrl = Request.Headers["Referer"].ToString();
                if (string.IsNullOrEmpty(referringUrl))
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    // Redirect back to the current URL
                    return Redirect(referringUrl);
                }
            }
        }

        private async Task<List<MovieInfo.Result>> GetMediaRecommendations(string media_type, int id, string userID)
        {
            // Use URL to get recommendations https://api.themoviedb.org/3/{media_type}/{id}/recommendations
            // Use MovieInfo Model for both tv and movies

            string apiUrl = $"https://api.themoviedb.org/3/{media_type}/{id}/recommendations?api_key=ca80dfbe1afe5a1a97e4401ff534c4e4";
            MovieInfo.Root movieInfo = await FetchMovies(apiUrl);

            if (movieInfo != null)
            {
                List<MovieInfo.Result> movieResults = ProcessMovieResults(movieInfo.results, userID);
                return movieResults;
            }

            return new List<MovieInfo.Result>();
        }




        private void TVShowViewBags(TvShowDetails.Root movie_tv_details)
        {
            TvShowDetails.LastEpisodeToAir season = movie_tv_details.last_episode_to_air;

            if (movie_tv_details.next_episode_to_air != null)
            {
                TvShowDetails.NextEpisodeToAir season_next_air = movie_tv_details.next_episode_to_air;
                DateTime nextReleaseDate = DateTime.Parse(season_next_air.air_date);
                TempData["NextAirDate"] = nextReleaseDate.ToString("dd/MM/yyyy");

                TempData["NextEpisodeName"] = season_next_air.name;
                TempData["season_next_air"] = season_next_air.air_date;




                DateTime nextReleaseDate2 = DateTime.Parse(movie_tv_details.next_episode_to_air.air_date);
                // Find the next release date that is in the future and falls on the same day of the week as the current date
                while (nextReleaseDate2 < DateTime.Now || nextReleaseDate2.DayOfWeek != DateTime.Now.DayOfWeek)
                {
                    nextReleaseDate2 = nextReleaseDate2.AddDays(1);
                }

                // Calculate the number of days remaining until the next release date
                TimeSpan timeRemaining = nextReleaseDate2 - DateTime.Now;

                if (timeRemaining.Ticks > 0)
                {
                    string remainingTime = $"{(int)timeRemaining.TotalDays}d {timeRemaining.Hours}h {timeRemaining.Minutes}m";
                    TempData["NextEpisodeRemainingTime"] = remainingTime;
                }
                else
                {
                    TempData["NextEpisodeRemainingTime"] = "Airing today";
                }
            }
            ConvertRuntime(season.runtime);

            var seasonInfo = movie_tv_details.seasons.OrderByDescending(s => s.season_number).FirstOrDefault();
            int episodeCount = seasonInfo?.episode_count ?? 0;
            int seasonCount = seasonInfo?.season_number ?? 0;

            int episodeTotalCount = movie_tv_details.seasons.Sum(s => s.episode_count);
            TempData["episodeTotalCount"] = (episodeTotalCount != null) ? episodeTotalCount : 0;
            TempData["SeasonNumber"] = (seasonCount != null) ? seasonCount : 1;
            TempData["SeasonEpisodeCount"] = (episodeCount != null) ? episodeCount : 0;
            var seasonrelease = (seasonInfo != null) ? seasonInfo?.air_date : "00:00:0000";
            ViewBag.SeasonRuntime = (seasonInfo != null) ? season?.runtime : 0;

            if (DateTime.TryParse(seasonrelease, out DateTime releaseDate))
            {
                ViewBag.ReleaseDate = releaseDate.Year.ToString();
            }
            else
            {
                ViewBag.ReleaseDate = "N/A";
            }

            DateTime initialRelease = DateTime.Parse(movie_tv_details.first_air_date);
            ViewBag.InitialReleaseDate = initialRelease.Year.ToString();
            ViewBag.FullReleaseDate = initialRelease.ToString("dd/MM/yyyy");

            DateTime finalRelease = DateTime.Parse(movie_tv_details.last_air_date);
            ViewBag.FinalReleaseDate = finalRelease.Year.ToString();

            CalculateAverageRating(movie_tv_details.vote_average);

            TempData["Status"] = movie_tv_details.status;

            if (movie_tv_details.status == "Ended" && movie_tv_details.seasons.Sum(s => s.season_number) >= 2)
            {
                ViewBag.SeriesEnded = "yes";
            }
            else
            {
                ViewBag.SeriesEnded = "no";
            }
        }
        private void MovieViewBags(TvShowDetails.Root movie_tv_details)
        {
            DateTime releaseDate = DateTime.Parse(movie_tv_details.release_date);
            ViewBag.ReleaseDate = releaseDate.Year.ToString();
            ViewBag.FullReleaseDate = releaseDate.ToString("dd/MM/yyyy");
            CalculateAverageRating(movie_tv_details.vote_average);

            TempData["Status"] = movie_tv_details.status;

            ConvertRuntime(movie_tv_details.runtime);
        }
        private void CalculateAverageRating(double movie_tv_details)
        {
            ViewBag.AverageRating = MathF.Round((float)movie_tv_details, 1); ;
            //double percentage = movie_tv_details * 10;
            //ViewBag.AverageRating = $"{percentage}%";

            //double rating = percentage / 100; 
            //ViewBag.Rating = rating.ToString("P0"); 

        }
        private async void ExtractPosterColour(string posterUrl)
        {
            using (var webClient = new WebClient())
            {
                byte[] imageData = webClient.DownloadData(posterUrl);
                using (var image = SixLabors.ImageSharp.Image.Load<Rgba32>(imageData))
                {
                    // Extract the dominant color from the poster image
                    var pixel = image[0, 0];
                    string dominantColor = $"#{pixel.R:X2}{pixel.G:X2}{pixel.B:X2}";

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

                // Skip the movie if it has no poster and no known_for data
                if (string.IsNullOrEmpty(movie.poster_path) && (movie.known_for == null || movie.known_for.Count == 0))
                {
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
                    ViewBag.IsMovieSaved = movie.IsSaved;
                }

                movieResults.Add(movie);
            }

            ViewBag.ResultList = results;
            ViewBag.MovieList = movieResults;

            return movieResults;
        }


        private bool MovieIsSaved(int movieId)
        {
            using (MovieDbContext _movieDbContext = new MovieDbContext())
            {
                string currentUserEmail = User.Identity.Name;
                User currentUser = _movieDbContext.Users.FirstOrDefault(x => x.Email == currentUserEmail);

                bool isSaved = _movieDbContext.UserMovies.Any(u => u.movie.TheMovieDbId == movieId && u.UserId == currentUser.UserId);
                return isSaved;
            }
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
            string formattedRuntime;

            formattedRuntime = (hours > 0) ? $"{hours}h {minutes}m" : $"{minutes}m";

            ViewBag.Runtime = formattedRuntime;
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

        private async Task<List<MediaWatchProviders.Buy>> GetMediaProviders(string media_type, int id)
        {
            string apiUrl = $"https://api.themoviedb.org/3/{media_type}/{id}/watch/providers?api_key=ca80dfbe1afe5a1a97e4401ff534c4e4";
            MediaWatchProviders.Root movieInfo = await FetchMediaProviders(apiUrl);

            if (movieInfo != null)
            {
                return await GetFilteredProviders(movieInfo);
            }
            return null;

        }

        private async Task<MediaWatchProviders.Root> FetchMediaProviders(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                var info = JsonConvert.DeserializeObject<MediaWatchProviders.Root>(json);

                return info;
            }
        }
        private async Task<List<MediaWatchProviders.Buy>> GetFilteredProviders(MediaWatchProviders.Root movieInfo)
        {
            if (movieInfo.results.US != null && movieInfo.results.GB != null)
            {
                if (movieInfo.results.US.flatrate != null || movieInfo.results.GB.flatrate != null)
                {
                    List<MediaWatchProviders.Flatrate> providers = movieInfo.results.US.flatrate
                    .Concat(movieInfo.results.GB.flatrate)
                    .GroupBy(p => p.provider_id) // Group providers by provider_id to remove duplicates
                    .Select(group => group.OrderBy(p => p.display_priority).First()) // Select the providers with the lowest display_priority
                    .OrderBy(p => p.display_priority)
                    .Take(5) // Take up to 5 providers with the lowest display_priority (most important)
                    .ToList();

                    ViewBag.Providers = providers;

                }
                else if (movieInfo.results.US.buy != null && movieInfo.results.GB.buy != null)
                {
                    List<MediaWatchProviders.Buy> providers = movieInfo.results.US.buy
                        .Concat(movieInfo.results.GB.buy)
                        .GroupBy(p => p.provider_id) // Group providers by provider_id to remove duplicates
                        .Select(group => group.OrderBy(p => p.display_priority).First()) // Select the providers with the lowest display_priority
                        .OrderBy(p => p.display_priority)
                        .Take(5) // Take up to 5 providers with the lowest display_priority (most important)
                        .ToList();

                    ViewBag.Providers = providers;

                    return providers;

                }
            }
            return null;
        }
    }
}
