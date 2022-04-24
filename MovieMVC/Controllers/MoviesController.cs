using Microsoft.AspNetCore.Mvc.Rendering;

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
            var movies = await _context.Movies.ToListAsync();
            return View(movies);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(int id)
        {
            if (_context.Movies.Any(m => m.MovieId == id))
            {
                var localMovie = await _context.Movies.FirstOrDefaultAsync(m => m.MovieId == id);
                return RedirectToAction("Details", "Movies", new { id = localMovie.Id, local = true });
            }

            var movieDetail = await _tmdbMovieService.MovieDetailAsync(id);

            var movie = await _tmdbMappingService.MapMovieDetailAsync(movieDetail);

            _context.Add(movie);
            await _context.SaveChangesAsync();

            await AddToMovieCollection(movie.Id, _appSettings.MovieMVCSettings.DefaultCollection.Name);

            return RedirectToAction(nameof(Import));

        }

        public async Task<IActionResult> Library()
        {
            var movies = await _context.Movies.ToListAsync();
            return View(movies);
        }

        public async Task<IActionResult> Details(int? id, bool local = false)
        {
            if (id is null)
            {
                return NotFound();
            }

            Movie movie = new();
            if (local)
            {
                //Get the Movie data straight from the DB
                movie = await _context.Movies.Include(m => m.Cast)
                                            .Include(m => m.Crew)
                                            .FirstOrDefaultAsync(m => m.Id == id);
            }
            else
            {
                //Get the movie data from the TMDB API
                var movieDetail = await _tmdbMovieService.MovieDetailAsync((int)id);
                movie = await _tmdbMappingService.MapMovieDetailAsync(movieDetail);
            }

            if (movie is null)
            {
                return NotFound();
            }

            ViewData["Local"] = local;
            return View(movie);
        }

        public IActionResult Create()
        {
            ViewData["CollectionId"] = new SelectList(_context.Collections, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,MovieId,Title,TagLine,Overview,RunTime,ReleaseDate,Rating,VoteAverage,Poster,PosterType,Backdrop,BackdropType,TrailerUrl")] Movie movie, int collectionId)
        {
            if (ModelState.IsValid)
            {
                movie.PosterType = movie.PosterFile?.ContentType;
                movie.Poster = await _imageService.EncodeImageAsync(movie.PosterFile);

                movie.BackdropType = movie.BackdropFile?.ContentType;
                movie.Backdrop = await _imageService.EncodeImageAsync(movie.BackdropFile);

                _context.Add(movie);
                await _context.SaveChangesAsync();


                await AddToMovieCollection(movie.Id, collectionId); 

                return RedirectToAction("Index", "MovieCollections");
            }
            return View(movie);
        }

        //Get the Edit page
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }
            return View(movie);
        }       

        // Post the Edit page

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,MovieId,Title,TagLine,Overview,RunTime,ReleaseDate,Rating,VoteAverage,Poster,PosterType,Backdrop,BackdropType,TrailerUrl")] Movie movie)
        {
            if (id != movie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (movie.PosterFile is not null)
                    {
                        movie.PosterType = movie.PosterFile.ContentType;
                        movie.Poster = await _imageService.EncodeImageAsync(movie.PosterFile);
                    }
                    if (movie.BackdropFile is not null)
                    {
                        movie.BackdropType = movie.BackdropFile.ContentType;
                        movie.Backdrop = await _imageService.EncodeImageAsync(movie.BackdropFile);
                    }


                    _context.Update(movie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MovieExists(movie.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Movies", new { id = movie.Id, local = true });
            }
            return View(movie);
        }
        // Get the delete page 
        public async Task<IActionResult> Delete(int? id)
        {
            if (id is null)
            {
                return NotFound();
            }

            var movie = await _context.Movies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (movie is null)
            {
                return NotFound();
            }

            return View(movie);
        }

        // Post the delete page

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
            return RedirectToAction("Library", "Movies");
        }

        private bool MovieExists(int id)
        {
            return _context.Movies.Any(e => e.Id == id);
        }       

        private async Task AddToMovieCollection(int id, string collectionName)
        {
            var collection = await _context.Collections.FirstOrDefaultAsync(c => c.Name == collectionName);

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
