namespace Tebing.ViewModels.Shared;

// Model buat partial _EmptyState: judul, 1 kalimat penjelasan, dan (opsional) 1 tombol.
public record EmptyStateViewModel(string Title, string Message, string? ActionText = null, string? ActionUrl = null);
