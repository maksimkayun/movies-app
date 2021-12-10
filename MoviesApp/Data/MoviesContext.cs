using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MoviesApp.Models;

namespace MoviesApp.Data
{
    public class MoviesContext : IdentityDbContext<ApplicationUser>
    {
        public MoviesContext (DbContextOptions<MoviesContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<MoviesArtist>(entity =>
            {
                entity.HasKey(sc => new {sc.MovieId, sc.ArtistId});
            });
        }

        public DbSet<Movie> Movies { get; set; }
        public DbSet<Artist> Artists { get; set; }
        public DbSet<MoviesArtist> MoviesArtists { get; set; }
    }
}