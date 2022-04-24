namespace MovieMVC.Services
{
    public class TMDBMappingService : IDataMappingService
    {
        private readonly AppSettings _appSettings;
        private readonly IImageService _imageService;
        public TMDBMappingService(IOptions<AppSettings> appSettings, IImageService imageService)
        {
            _appSettings = appSettings.Value;
            _imageService = imageService;
        }
        public ActorDetail MapActorDetail(ActorDetail actor)
        {
            actor.profile_path = BuildCastImage(actor.profile_path);

            if(string.IsNullOrEmpty(actor.biography))
                actor.biography = "No biography available";

            if(string.IsNullOrEmpty(actor.place_of_birth))
                actor.place_of_birth = "No place of birth available";

            if (string.IsNullOrEmpty(actor.birthday))
                actor.birthday = "No birth available";
            
            else
                actor.birthday = DateTime.Parse(actor.birthday).ToString("dd MMM yyyy");

            return actor;
        }

        public async Task<Movie> MapMovieDetailAsync(MovieDetail movie)
        {
            Movie newMovie = null;
            try
            {
                newMovie = new Movie
                {
                    MovieId = movie.id,
                    Title = movie.title,
                    TagLine = movie.tagline,
                    Overview = movie.overview,
                    RunTime = movie.runtime,
                    VoteAverage = movie.vote_average,
                    ReleaseDate = DateTime.Parse(movie.release_date),
                    TrailerUrl = BuildTrailerPath(movie.videos),
                    Backdrop = await EncodeBackdropImageAsync(movie.backdrop_path),
                    BackdropType = BuildImageType(movie.backdrop_path),
                    Poster = await EncodePosterImageAsync(movie.poster_path),
                    PosterType = BuildImageType(movie.poster_path),
                    Rating = GetRating(movie.release_dates)
                };

            }
            catch (Exception ex)
            {

                Console.WriteLine($"Something went wrong in MapMovieDetailAsync: {ex.Message}");
            }

            var castMembers = movie.credits.cast.OrderByDescending(c => c.popularity)
                                                .GroupBy(c => c.cast_id)
                                                .Select(g => g.FirstOrDefault())
                                                .Take(20)
                                                .ToList();

            castMembers.ForEach(member =>
            {
                newMovie.Cast.Add(new MovieCast()
                {
                    CastID = member.id,
                    Department = member.known_for_department,
                    Name = member.name,
                    Character = member.character,
                    ImageUrl = BuildCastImage(member.profile_path)
                });
            });


            var crewMembers = movie.credits.crew.OrderByDescending(c => c.popularity)
                                                .GroupBy(c => c.id)
                                                .Select(g => g.First())
                                                .Take(20)
                                                .ToList();
            crewMembers.ForEach(member =>
            {
                newMovie.Crew.Add(new MovieCrew()
                {
                    CrewID = member.id,
                    Department = member.known_for_department,
                    Name = member.name,
                    Job = member.job,
                    ImageUrl = BuildCastImage(member.profile_path)
                });
            });

            return newMovie;
        }

        private async Task<byte[]> EncodePosterImageAsync(string poster_path)
        {
            var posterPath = $"{_appSettings.TmDbSettings.BaseImagePath}/{_appSettings.MovieMVCSettings.DefaultPosterSize}/{poster_path}";

            return await _imageService.EncodeImageUrlAsync(posterPath);
        }

        private string BuildCastImage(string profile_path)
        {
            if (string.IsNullOrEmpty(profile_path))
                return _appSettings.MovieMVCSettings.DefaultCastImage;

            return $"{_appSettings.TmDbSettings.BaseImagePath}/{_appSettings.MovieMVCSettings.DefaultPosterSize}/{profile_path}";
        }

        private MovieRating GetRating(Release_Dates dates)
        {

            var movieRating = MovieRating.NR;
            var certification = dates.results.FirstOrDefault(x => x.iso_3166_1 == "US");

            if (certification is not null)
            {
                var apiRating = certification.release_dates.FirstOrDefault(c => c.certification != "")?.certification.Replace("-", "");
                if (!string.IsNullOrEmpty(apiRating))
                {
                    movieRating = (MovieRating)Enum.Parse(typeof(MovieRating), apiRating, true);
                }                
            }

            return movieRating;
        }       

        private string BuildImageType(string backdrop_path)
        {
            if (string.IsNullOrEmpty(backdrop_path))
                return backdrop_path;
            
            return $"image/{Path.GetExtension(backdrop_path).TrimStart('.')}";
        }

        private string BuildTrailerPath(Videos videos)
        {
            var videoKey = videos.results.FirstOrDefault(r => r.type.ToLower().Trim() == "trailer" && r.key != "")?.key;

            return string.IsNullOrEmpty(videoKey) ? videoKey : $"{_appSettings.TmDbSettings.BaseYoutubePath}{videoKey}";
        }

        private async Task<byte[]> EncodeBackdropImageAsync(string path)
        {
            var backdropPath = $"{_appSettings.TmDbSettings.BaseImagePath}/{_appSettings.MovieMVCSettings.DefaultBackdropSize}/{path}";

            return await _imageService.EncodeImageUrlAsync(backdropPath);
        }
    }
}
