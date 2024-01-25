using Microsoft.EntityFrameworkCore;
using MovieApp.Data.Models;

namespace MovieApp.Data.Context
{
    public interface IMovieDbContext
    {
        DbSet<Movie> Movies { get; set; }
        DbSet<User> Users { get; set; }

        int SaveChanges();
    }
}