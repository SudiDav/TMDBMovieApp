

namespace MovieMVC.Controllers
{
    public class MovieCollectionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MovieCollectionsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int? id)
        {
            id ??= (await _context.Collections.FirstOrDefaultAsync(c => c.Name.ToUpper() == "ALL")).Id;

            ViewData["CollectionId"] = new SelectList(_context.Collections, "Id", "Name", id);

            var allMovieIds = await _context.Movies.Select(m => m.Id).ToListAsync();

            var movieIdsInCollection = await _context.MovieCollections
                                        .Where(m => m.CollectionId == id)
                                        .OrderBy(m => m.Order)
                                        .Select(m => m.MovieId)
                                        .ToListAsync();
            var movieIdsNotInCollection = allMovieIds.Except(movieIdsInCollection);

            var moviesInCollection = new List<Movie>();
            movieIdsInCollection.ForEach(movieId => moviesInCollection.Add(_context.Movies.Find(movieId)));

            ViewData["IdsInCollection"] = new MultiSelectList(moviesInCollection, "Id", "Title");

            var moviesNotInCollection = await _context.Movies.AsNoTracking().Where(m => movieIdsNotInCollection.Contains(m.Id)).ToListAsync();
            ViewData["IdsNotInCollection"] = new MultiSelectList(moviesNotInCollection, "Id", "Title");

            return View();

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(int id, List<int> idsInCollection)
        {
            var oldRecords = _context.MovieCollections.Where(c => c.CollectionId == id);
            _context.MovieCollections.RemoveRange(oldRecords);
            await _context.SaveChangesAsync();

            if (idsInCollection is not null)
            {
                int index = 1;
                idsInCollection.ForEach(movieId =>
                {
                    _context.Add(new MovieCollection()
                    {
                        CollectionId = id,
                        MovieId = movieId,
                        Order = index++
                    });
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index), new { id });
        }        
    }
}
