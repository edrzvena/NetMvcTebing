namespace Tebing.ViewModels.Shared;

// Ukuran thumbnail foto gunung di halaman booking (docs/UI_UX.md §5.4–5.6).
public enum ThumbSize
{
    Wide,   // ringkasan Buat Booking: lebar penuh, 16:9
    Small,  // kartu Booking Saya di HP: 64×64
    Medium  // header Detail Booking: 96×64
}

// ImagePath null = gunung tanpa foto → placeholder ikon (bukan <img> rusak).
public record MountainThumbViewModel(string? ImagePath, string Name, ThumbSize Size);
