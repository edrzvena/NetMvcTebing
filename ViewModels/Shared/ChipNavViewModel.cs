namespace Tebing.ViewModels.Shared;

// Chip = link filter (bukan tombol JS). Count null = gak nampilin angka.
public record ChipItem(string Label, int? Count, string Url, bool Active);

public record ChipNavViewModel(string AriaLabel, List<ChipItem> Items);
