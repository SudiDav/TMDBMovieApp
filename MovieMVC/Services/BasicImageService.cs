﻿namespace MovieMVC.Services
{
    public class BasicImageService : IImageService
    {
        private readonly IHttpClientFactory _httpClient;
        public BasicImageService(IHttpClientFactory httpClient)
        {
            _httpClient = httpClient;
        }
        public string DecodeImage(byte[] poster, string contentType)
        {
            if (poster == null) return null;
            var posterImage = Convert.ToBase64String(poster);
            return $"data:{contentType};base64,{posterImage}";

        }

        public async Task<byte[]> EncodeImageAsync(IFormFile poster)
        {
            if (poster == null) return null;

            using var memoryStream = new MemoryStream();
            await poster.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
        
        public async Task<byte[]> EncodeImageUrlAsync(string imageUrl)
        {
            var client = _httpClient.CreateClient();
            var response = await client.GetAsync(imageUrl);
            using Stream stream = await response.Content.ReadAsStreamAsync();
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
    }
}
