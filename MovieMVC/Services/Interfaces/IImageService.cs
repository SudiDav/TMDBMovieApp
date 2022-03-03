namespace MovieMVC.Services.Interfaces
{
    public interface IImageService
    {
        Task<byte[]> EncodingImageAsync(IFormFile poster);
        Task<byte[]> EncodingImageuUrlAsync(string ImageUrl);
        string DecodeImage(byte[] poster, string contentType);
    }
}
