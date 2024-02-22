
namespace EventSharedExpenseTracker.Application.Interfaces;

public interface IImageService
{
    Task<string> UpdateImageAsync(Stream imageFileStream, string imagePath);
    void DeleteImageFile(string imagePath);
}
