using Microsoft.AspNetCore.WebUtilities;
using System.Runtime.Serialization.Json;

namespace MovieMVC.Services
{
    public class TMDBMovieService : IRemoteMovieService
    {
        private readonly AppSettings _appSettings;
        private readonly IHttpClientFactory _httpClient;

        public TMDBMovieService(IOptions<AppSettings> appSettings, IHttpClientFactory httpClient)
        {
            _appSettings = appSettings.Value;
            _httpClient = httpClient;
        }

        public async Task<ActorDetail> ActorDetailAsync(int id)
        {
            ActorDetail actorDetail = new();

            var queryString = $"{_appSettings.TmDbSettings.BaseUrl}/person/{id}";

            var queryParams = new Dictionary<string, string>()
            {
                { "api_key", _appSettings.MovieMVCSettings.TmDbApiKey },
                { "language", _appSettings.TmDbSettings.QueryOptions.Language },
            };

            var requestUri = QueryHelpers.AddQueryString(queryString, queryParams);

            var client = _httpClient.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();

                var dataContractJsonSerializer = new DataContractJsonSerializer(typeof(ActorDetail));

                actorDetail = (ActorDetail)dataContractJsonSerializer.ReadObject(responseStream);
            }

            return actorDetail;
        }

        public async Task<MovieDetail> MovieDetailAsync(int id)
        {
            MovieDetail movieDetail = new();

            var queryString = $"{_appSettings.TmDbSettings.BaseUrl}/movie/{id}";

            var queryParams = new Dictionary<string, string>()
            {
                { "api_key", _appSettings.MovieMVCSettings.TmDbApiKey },
                { "language", _appSettings.TmDbSettings.QueryOptions.Language },
                { "append_to_response", _appSettings.TmDbSettings.QueryOptions.AppendToResponse }
            };

            var requestUri = QueryHelpers.AddQueryString(queryString, queryParams);

            var client = _httpClient.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = await client.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                using var responseStream = await response.Content.ReadAsStreamAsync();
                var dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MovieDetail));
                movieDetail = dataContractJsonSerializer.ReadObject(responseStream) as MovieDetail;
            }

            return movieDetail;
        }

        public async Task<MovieSearch> SearchMovieAsync(MovieCategory category, int count)
        {
            MovieSearch movieSearch = new();

            var queryString = $"{_appSettings.TmDbSettings.BaseUrl}/movie/{category}";

            var queryParams = new Dictionary<string, string>
            {
                { "api_key", _appSettings.MovieMVCSettings.TmDbApiKey },
                { "language", _appSettings.TmDbSettings.QueryOptions.Language },
                { "page", _appSettings.TmDbSettings.QueryOptions.Page },
            };

            var requestUri = QueryHelpers.AddQueryString(queryString, queryParams);

            var client = _httpClient.CreateClient();
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var dataContractJsonSerializer = new DataContractJsonSerializer(typeof(MovieSearch));
                using var responseStream = await response.Content.ReadAsStreamAsync();
                movieSearch = (MovieSearch)dataContractJsonSerializer.ReadObject(responseStream);
                movieSearch.results = movieSearch.results.Take(count).ToArray();
                movieSearch.results.ToList().ForEach(r => r.poster_path = $"{_appSettings.TmDbSettings.BaseImagePath}/{_appSettings.MovieMVCSettings.DefaultPosterSize}/{r.poster_path}");
            }

            return movieSearch;
        }
    }
}
