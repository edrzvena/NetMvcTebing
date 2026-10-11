namespace Tebing.Common.Regions;

// Urutan enum = urutan chip pulau di UI (docs/UI_UX.md §4.3).
public enum Island
{
    Jawa,
    BaliNusaTenggara,
    Sumatra,
    Kalimantan,
    Sulawesi,
    MalukuPapua
}

// Data gunung cuma nyimpen provinsi (teks bebas dari admin), jadi pulau diturunin di sini — satu-satunya
// tempat mapping provinsi → pulau. Provinsi yang gak dikenal = null: gunungnya tetap tampil di "Semua",
// cuma gak masuk chip pulau mana pun.
public static class IslandRegion
{
    private static readonly (string Prefix, Island Island)[] Prefixes =
    [
        ("Jawa", Island.Jawa),
        ("Sumatera", Island.Sumatra),
        ("Sumatra", Island.Sumatra),
        ("Kalimantan", Island.Kalimantan),
        ("Sulawesi", Island.Sulawesi),
        ("Maluku", Island.MalukuPapua),
        ("Papua", Island.MalukuPapua),
        ("Nusa Tenggara", Island.BaliNusaTenggara)
    ];

    private static readonly Dictionary<string, Island> ExactNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Banten"] = Island.Jawa,
        ["DKI Jakarta"] = Island.Jawa,
        ["Jakarta"] = Island.Jawa,
        ["DI Yogyakarta"] = Island.Jawa,
        ["Daerah Istimewa Yogyakarta"] = Island.Jawa,
        ["Yogyakarta"] = Island.Jawa,
        ["Aceh"] = Island.Sumatra,
        ["Riau"] = Island.Sumatra,
        ["Kepulauan Riau"] = Island.Sumatra,
        ["Jambi"] = Island.Sumatra,
        ["Bengkulu"] = Island.Sumatra,
        ["Lampung"] = Island.Sumatra,
        ["Bangka Belitung"] = Island.Sumatra,
        ["Kepulauan Bangka Belitung"] = Island.Sumatra,
        ["Bali"] = Island.BaliNusaTenggara,
        ["NTB"] = Island.BaliNusaTenggara,
        ["NTT"] = Island.BaliNusaTenggara,
        ["Gorontalo"] = Island.Sulawesi
    };

    public static Island? FromProvince(string? province)
    {
        var name = province?.Trim();
        if (string.IsNullOrEmpty(name))
            return null;

        if (ExactNames.TryGetValue(name, out var island))
            return island;

        foreach (var (prefix, prefixIsland) in Prefixes)
        {
            if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                return prefixIsland;
        }

        return null;
    }

    public static string ToLabel(this Island island) => island switch
    {
        Island.Jawa => "Jawa",
        Island.BaliNusaTenggara => "Bali & Nusa Tenggara",
        Island.Sumatra => "Sumatra",
        Island.Kalimantan => "Kalimantan",
        Island.Sulawesi => "Sulawesi",
        Island.MalukuPapua => "Maluku & Papua",
        _ => island.ToString()
    };
}
