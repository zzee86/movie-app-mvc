using Microsoft.AspNetCore.Mvc;
using movie_app_mvc.Models;

namespace movie_app_mvc.Controllers
{
    public interface IDetailsController
    {
        Task<ActionResult> MovieDetails(string title, int id);
    }
}