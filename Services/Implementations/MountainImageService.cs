using Tebing.Common.Images;
using Tebing.Constants;
using Tebing.Services.Interfaces;

namespace Tebing.Services.Implementations;

public class MountainImageService(IWebHostEnvironment environment) : IMountainImageService
{
    public const string UploadRequestPath = "/uploads/mountains";

    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];

    private string UploadFolder => Path.Combine(environment.WebRootPath, "uploads", "mountains");

    public async Task<ImageSaveResult> SaveAsync(IFormFile file)
    {
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return new ImageSaveResult(false, "Foto harus JPG, PNG, atau WebP.");

        if (file.Length == 0 || file.Length > FileUploadSettings.MountainImageMaxBytes)
            return new ImageSaveResult(false, "Ukuran foto maksimal 5 MB.");

        // Ekstensi bisa dipalsuin — yang nentuin tetap isi file-nya bisa di-decode jadi gambar atau gak.
        using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer);
        buffer.Position = 0;

        var resized = MountainImageProcessor.Process(buffer);
        if (resized is null)
            return new ImageSaveResult(false, "File itu bukan gambar yang bisa dibaca.");

        // Nama unik & gak pernah ditimpa, jadi aman di-cache browser selamanya (lihat Program.cs).
        Directory.CreateDirectory(UploadFolder);
        var name = Guid.NewGuid().ToString("N");
        foreach (var (width, bytes) in resized)
            await File.WriteAllBytesAsync(Path.Combine(UploadFolder, $"{name}-{width}.webp"), bytes);

        return new ImageSaveResult(true, ImagePath: $"{UploadRequestPath}/{name}");
    }

    public void Delete(string? imagePath)
    {
        if (imagePath is null || !imagePath.StartsWith(UploadRequestPath + "/"))
            return;

        // Path dari database, tapi tetap ambil nama file-nya aja biar gak bisa keluar dari folder upload.
        var name = Path.GetFileName(imagePath);
        foreach (var width in MountainImageProcessor.Widths)
        {
            var fullPath = Path.Combine(UploadFolder, $"{name}-{width}.webp");
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }
}
