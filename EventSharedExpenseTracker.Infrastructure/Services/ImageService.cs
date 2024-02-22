using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Jpeg;
using EventSharedExpenseTracker.Application.Interfaces;

namespace EventSharedExpenseTracker.Infrastructure.Services;

public class ImageService : IImageService
{
    public async Task<string> UpdateImageAsync(Stream imageFileStream, string currentImagePath)
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
            }
        }
    }

    private static async Task UploadImageAsync(Stream imageFileStream, string filePath)
    {
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await imageFileStream.CopyToAsync(stream);
        }

        // Resize and compress the uploaded image
        ResizeAndCompressImage(filePath);
    }

    private static void ResizeAndCompressImage(string filePath)
    {
        using (var image = Image.Load(filePath))
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
            image.Save(filePath, jpegEncoder);
        }
    }

    private static string GetExtension(string contentType)
    {
        return contentType switch
        {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            "image/gif" => ".gif",
            // Add more cases for other image formats if needed
            _ => string.Empty,
        };
    }
}
