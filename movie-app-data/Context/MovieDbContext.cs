﻿using Microsoft.EntityFrameworkCore;
using MovieApp.Data.Models;

namespace MovieApp.Data.Context
{
    public class MovieDbContext : DbContext, IMovieDbContext
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
                optionsBuilder.UseNpgsql("Host=localhost; Database=postgres; Username=postgres; Password=password123");
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
        public int SaveChanges()
        {
            return base.SaveChanges();
        }
    }
}
