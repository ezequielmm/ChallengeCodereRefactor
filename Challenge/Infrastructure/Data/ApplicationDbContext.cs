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
                .HasKey(c => c.Code); // Establece 'Code' como clave primaria
            modelBuilder.Entity<Country>()
                .Property(c => c.Code)
                .ValueGeneratedNever(); // Indica que 'Code' no es generado por la base de datos

            // Configuración de Network
            modelBuilder.Entity<Network>()
                .HasKey(n => n.Id);
            modelBuilder.Entity<Network>()
                .Property(n => n.Id)
                .ValueGeneratedOnAdd(); // 'Id' generado por la base de datos
            modelBuilder.Entity<Network>()
                .HasOne(n => n.Country)
                .WithMany()
                .HasForeignKey(n => n.CountryCode)
                .OnDelete(DeleteBehavior.Restrict);

            // Configuración de Show
            modelBuilder.Entity<Show>()
                .HasKey(s => s.Id);
            modelBuilder.Entity<Show>()
                .Property(s => s.Id)
                .ValueGeneratedOnAdd(); // 'Id' generado por la base de datos

            // Configuración de Externals
            modelBuilder.Entity<Externals>()
                .HasKey(e => e.Id);
            modelBuilder.Entity<Externals>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd(); // 'Id' generado por la base de datos
            modelBuilder.Entity<Externals>()
                .HasOne(e => e.Show)
                .WithOne(s => s.Externals)
                .HasForeignKey<Externals>(e => e.ShowId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuración de Rating
            modelBuilder.Entity<Rating>()
                .HasKey(r => r.Id);
            modelBuilder.Entity<Rating>()
                .Property(r => r.Id)
                .ValueGeneratedOnAdd(); // 'Id' generado por la base de datos
            modelBuilder.Entity<Rating>()
                .HasOne(r => r.Show)
                .WithOne(s => s.Rating)
                .HasForeignKey<Rating>(r => r.ShowId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configuración de Genre
            modelBuilder.Entity<Genre>()
                .HasKey(g => g.Id);
            modelBuilder.Entity<Genre>()
                .Property(g => g.Id)
                .ValueGeneratedOnAdd(); // 'Id' generado por la base de datos

            // Relación Show -> Genre (muchos a muchos)
            modelBuilder.Entity<Show>()
                .HasMany(s => s.Genres)
                .WithMany(g => g.Shows)
                .UsingEntity(j => j.ToTable("ShowGenres"));
        }
    }
}
