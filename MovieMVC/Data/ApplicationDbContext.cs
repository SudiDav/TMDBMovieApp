namespace MovieMVC.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Collection> Collections { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<MovieCollection> MovieCollections { get; set; }
        
    }
}
