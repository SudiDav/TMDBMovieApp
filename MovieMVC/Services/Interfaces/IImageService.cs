namespace MovieMVC.Services.Interfaces
{
    public interface IImageService
    {
        Task<byte[]> EncodeImageAsync(IFormFile poster);
        Task<byte[]> EncodeImageUrlAsync(string ImageUrl);
        string DecodeImage(byte[] poster, string contentType);
    }
}
