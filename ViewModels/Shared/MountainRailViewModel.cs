using Tebing.DTOs.Mountain;

namespace Tebing.ViewModels.Shared;

// Satu baris kartu gunung yang bisa digeser (docs/UI_UX.md §4.3). Mountains kosong → rail gak dirender.
public record MountainRailViewModel(string Id, string Eyebrow, string Title, List<MountainListDto> Mountains,
    string? SeeAllUrl);
