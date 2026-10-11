namespace Tebing.Common.Text;

public static class TextExtensions
{
    // Kalimat pertama, buat tagline di banner Home. Akhir kalimat = . ! ? yang diikuti spasi atau akhir
    // teks — titik di angka kayak "2.958" gak dianggap akhir kalimat. Teks tanpa titik dikembaliin utuh.
    public static string? FirstSentence(this string? text)
    {
        var trimmed = text?.Trim();
        if (string.IsNullOrEmpty(trimmed))
            return null;

        for (var i = 0; i < trimmed.Length; i++)
        {
            if (trimmed[i] is '.' or '!' or '?' && (i == trimmed.Length - 1 || char.IsWhiteSpace(trimmed[i + 1])))
                return trimmed[..(i + 1)];
        }

        return trimmed;
    }
}
