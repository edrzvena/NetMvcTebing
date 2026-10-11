namespace Tebing.Services.Interfaces;

public record ImageSaveResult(bool Success, string? Error = null, string? ImagePath = null);

// Nyimpen foto gunung upload admin: validasi, resize ke WebP 640/1200, simpan di wwwroot/uploads/mountains.
public interface IMountainImageService
{
    Task<ImageSaveResult> SaveAsync(IFormFile file);

    // Cuma hapus foto hasil upload — foto seed di /images/mountains/ ikut repo, gak pernah dihapus.
    void Delete(string? imagePath);
}
