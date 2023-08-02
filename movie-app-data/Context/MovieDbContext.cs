using Microsoft.EntityFrameworkCore;
using MovieApp.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieApp.Data.Context
{
    public class MovieDbContext : DbContext
    {
        public MovieDbContext()
        {
        }

        public MovieDbContext(DbContextOptions<MovieDbContext> options) : base(options) { }

        public DbSet<Movie> Movies { get; set; }

        public DbSet<User> Users { get; set; }

        //public DbSet<User_Movie> UserMovies { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=Movie_App;Trusted_Connection=True;");
            }
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //modelBuilder.Entity<Movie>()
            //    .HasMany(m => m.UserMovies)
            //    .WithOne(om => om.movie)
            //    .HasForeignKey(fk => fk.MovieId);

            //modelBuilder.Entity<User>()
            //    .HasMany(m => m.UserMovies)
            //    .WithOne(ou => ou.user)
            //    .HasForeignKey(fk => fk.UserId);

        }
    }
}
