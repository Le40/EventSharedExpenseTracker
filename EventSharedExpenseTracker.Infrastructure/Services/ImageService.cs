using EventSharedExpenseTracker.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace EventSharedExpenseTracker.Infrastructure.Services;

public class ImageService : IImageService
{
    private readonly ILogger<ImageService> _logger;

    public ImageService(ILogger<ImageService> logger)
    {
        _logger = logger;
    }

    public async Task<string> SaveImageAsync(Stream imageFileStream, string currentImagePath)
    {
        if (imageFileStream == null || imageFileStream.Length == 0)
        {
            return currentImagePath;
        }

        string uniqueFileName = Guid.NewGuid().ToString()+ ".jpg";
        string filePath = Path.Combine("wwwroot/media", uniqueFileName);
        string newImagePath = "/media/" + uniqueFileName;

        await UploadImageAsync(imageFileStream, filePath);

        if (currentImagePath != null)
        {
            DeleteImageFile(currentImagePath);
        }
        return newImagePath;
    }

    public void DeleteImageFile(string imagePath)
    {
        // Ensure the imagePath is not null or empty
        if (!string.IsNullOrEmpty(imagePath))
        {
            // Get the full path to the image file
            string filePath = Path.Combine("wwwroot", imagePath.TrimStart('/'));

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Image deleted from {ImagePath}", imagePath);
            }
        }
    }

    private async Task UploadImageAsync(Stream imageFileStream, string filePath)
    {
        using (var image = Image.Load(imageFileStream))
        {
            // Resize the image to fit within 1980x1080 bounds while preserving aspect ratio
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Size = new Size(1980, 1080),
                Mode = ResizeMode.Max
            }));

            // Compress the image
            var jpegEncoder = new JpegEncoder
            {
                Quality = 70 // Adjust the quality level here (0-100)
            };
            // Save the processed image to a byte array
            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await image.SaveAsync(stream, jpegEncoder);
                _logger.LogInformation("Image uploaded to {ImagePath}", filePath);
            }
        }
    }
}
