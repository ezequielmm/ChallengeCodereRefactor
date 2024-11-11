using Microsoft.EntityFrameworkCore;
using Challenge.Domain.Entities;

namespace Challenge.Infrastructure.Data
{
    /// <summary>
    /// Contexto de la base de datos para la aplicación.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        /// <summary>
        /// Constructor que configura las opciones del contexto.
        /// </summary>
        /// <param name="options">Opciones de configuración del contexto.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets para las entidades
        public DbSet<Show> Shows { get; set; }
        public DbSet<Network> Networks { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<Externals> Externals { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<Rating> Ratings { get; set; }

        /// <summary>
        /// Configura el modelo mediante Fluent API.
        /// </summary>
        /// <param name="modelBuilder">Constructor del modelo.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración de Country
            modelBuilder.Entity<Country>()
                .HasKey(c => c.Code);
            modelBuilder.Entity<Country>()
                .Property(c => c.Code)
                .ValueGeneratedNever();

            // Configuración de Network
            modelBuilder.Entity<Network>()
                .HasKey(n => n.Id);
            modelBuilder.Entity<Network>()
                .Property(n => n.Id)
                .ValueGeneratedNever();
            modelBuilder.Entity<Network>()
                .HasOne(n => n.Country)
                .WithMany()
                .HasForeignKey(n => n.CountryCode)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación Network -> Shows (uno a muchos)
            modelBuilder.Entity<Network>()
                .HasMany(n => n.Shows)
                .WithOne(s => s.Network)
                .HasForeignKey(s => s.NetworkId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de Show
            modelBuilder.Entity<Show>()
                .HasKey(s => s.Id);
            modelBuilder.Entity<Show>()
                .Property(s => s.Id)
                .ValueGeneratedNever();

            // Configuración de Externals
            modelBuilder.Entity<Externals>()
                .HasKey(e => e.Id);
            modelBuilder.Entity<Externals>()
                .Property(e => e.Id)
                .ValueGeneratedNever();
            modelBuilder.Entity<Externals>()
                .Property(e => e.Imdb)
                .IsRequired(false); // Permite nulos en Imdb
            modelBuilder.Entity<Externals>()
                .HasOne(e => e.Show)
                .WithOne(s => s.Externals)
                .HasForeignKey<Externals>(e => e.Id);

            // Configuración de Rating
            modelBuilder.Entity<Rating>()
                .HasKey(r => r.Id);
            modelBuilder.Entity<Rating>()
                .Property(r => r.Id)
                .ValueGeneratedNever();
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Show)
                .WithOne(s => s.Rating)
                .HasForeignKey<Rating>(r => r.Id);

            // Configuración de Genre
            modelBuilder.Entity<Genre>()
                .HasKey(g => g.Id);
            modelBuilder.Entity<Genre>()
                .Property(g => g.Id)
                .ValueGeneratedOnAdd();

            // Relación Show -> Genre (muchos a muchos)
            modelBuilder.Entity<Show>()
                .HasMany(s => s.Genres)
                .WithMany(g => g.Shows)
                .UsingEntity(j => j.ToTable("ShowGenres"));
        }
    }
}
