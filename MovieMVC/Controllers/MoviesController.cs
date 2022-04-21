namespace MovieMVC.Controllers
{
    public class MoviesController : Controller
    {
        private readonly AppSettings _appSettings;
        private readonly ApplicationDbContext _context;
        private readonly IImageService _imageService;
        private readonly IRemoteMovieService _tmdbMovieService;
        private readonly IDataMappingService _tmdbMappingService;

        public MoviesController(IOptions<AppSettings> appSettings, 
            ApplicationDbContext context, 
            IImageService imageService, 
            IRemoteMovieService tmdbMovieService, 
            IDataMappingService tmdbMappingService)
        {
            _appSettings = appSettings.Value;
            _context = context;
            _imageService = imageService;
            _tmdbMovieService = tmdbMovieService;
            _tmdbMappingService = tmdbMappingService;
        }

        public async Task<IActionResult> Import()
        {
            var movies = await _context.Movie.ToListAsync();
            return View(movies);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(int id)
        {
            if (_context.Movie.Any(m => m.MovieId == id))
            {
                var localMovie = await _context.Movie.FirstOrDefaultAsync(m => m.MovieId == id);
                return RedirectToAction("Details", "Movies", new { id = localMovie.Id, local = true });
            }

            var movieDetail = await _tmdbMovieService.MovieDetailAsync(id);

            var movie = await _tmdbMappingService.MapMovieDetailAsync(movieDetail);

            _context.Add(movie);
            await _context.SaveChangesAsync();

            await AddToMovieCollection(movie.Id, _appSettings.MovieMVCSettings.DefaultCollection.Name);

            return RedirectToAction(nameof(Import));

        }

        private async Task AddToMovieCollection(int id, string collectionName)
        {
            var collection = await _context.Collection.FirstOrDefaultAsync(c => c.Name == collectionName);

            _context.Add(new MovieCollection()
            {
                MovieId = id,
                CollectionId = collection.Id
            });

            await _context.SaveChangesAsync();
        }

        private async Task AddToMovieCollection(int id, int collectionId)
        {
            _context.Add(new MovieCollection()
            {
                MovieId = id,
                CollectionId = collectionId
            });

            await _context.SaveChangesAsync();
        }
    }
}
