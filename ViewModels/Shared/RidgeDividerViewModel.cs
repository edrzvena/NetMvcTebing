namespace Tebing.ViewModels.Shared;

// Varian bentuk pembatas punggungan (docs/UI_UX.md §4.15): A = landai-panjang, B = lebih runcing & rapat.
public enum RidgeVariant
{
    A,
    B
}

// Flip = dicerminin horizontal, biar 2 pembatas di 1 halaman gak kelihatan copy-paste.
public record RidgeDividerViewModel(RidgeVariant Variant = RidgeVariant.A, bool Flip = false);
