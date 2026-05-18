namespace EventSharedExpenseTracker.Application.Common.Interfaces;

public interface IImageService
{
    Task<string> SaveImageAsync(Stream imageFileStream, string imagePath);
    void DeleteImageFile(string imagePath);
}
