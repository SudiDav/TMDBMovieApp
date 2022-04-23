namespace MovieMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly IRemoteMovieService _tmdbMovieService;

        public HomeController(ILogger<HomeController> logger, 
            ApplicationDbContext context, 
            IRemoteMovieService tmdbMovieService)
        {
            _logger = logger;
            _context = context;
            _tmdbMovieService = tmdbMovieService;
        }

        public async Task<IActionResult> Index()
        {
            const int count = 16;
            var data = new LandingPageVM()
            {                
                CustomCollections = await _context.Collection
                                            .Include(c => c.MovieCollections)
                                            .ThenInclude(mc => mc.Movie)
                                            .ToListAsync(),
                
                NowPlaying = await _tmdbMovieService.SearchMoviesAsync(MovieCategory.now_playing, count),
                Popular = await _tmdbMovieService.SearchMoviesAsync(MovieCategory.popular, count),
                TopRated = await _tmdbMovieService.SearchMoviesAsync(MovieCategory.top_rated, count),
                UpComing = await _tmdbMovieService.SearchMoviesAsync(MovieCategory.upcoming, count)
            };
            
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
