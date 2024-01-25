using Microsoft.AspNetCore.Mvc;

namespace movie_app_mvc.Controllers
{
    public interface IDetailsController
    {
        Task<ActionResult> MovieDetails(string title, int id);
    }
}