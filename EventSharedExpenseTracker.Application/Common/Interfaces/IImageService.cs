namespace EventSharedExpenseTracker.Application.Common.Interfaces;

public interface IImageService
{
    Task<string> SaveImageAsync(Stream imageFileStream, string imagePath);
    void DeleteImageFile(string imagePath);
    Task<byte[]> ResizeAndCompressAsync(
            Stream imageStream,
            int maxWidth,
            int maxHeight,
            int quality);
}
