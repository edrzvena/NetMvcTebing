using Tebing.Common.Enums;
using Tebing.Common.Regions;
using Tebing.DTOs.Mountain;

namespace Tebing.Common.Feeds;

public record IslandCount(Island Island, int Count);

public record HomeFeed(
    List<MountainListDto> Featured,
    List<MountainListDto> Picks,
    List<MountainListDto> Beginner,
    List<MountainListDto> Challenging,
    List<IslandCount> Islands);

// Aturan milih gunung buat banner, rail, & chip (docs/UI_UX.md §4.1, §4.3, §5.1, §5.3). Sengaja pure —
// cuma ngolah list yang udah diambil service — biar bisa dites tanpa database.
public static class MountainFeed
{
    private const int FeaturedCount = 5;
    private const int RailCount = 10;

    public static HomeFeed BuildHome(IReadOnlyList<MountainListDto> all)
    {
        var open = all.Where(m => m.Status == MountainStatus.Open).ToList();

        // Tanpa ulasan sama sekali, yang tertinggi duluan — biar banner tetap isi gunung ikonik.
        var byHighlight = open
            .OrderByDescending(m => m.AverageRating ?? 0)
            .ThenByDescending(m => m.ReviewCount)
            .ThenByDescending(m => m.HeightMeters)
            .ToList();

        var featured = byHighlight.Where(m => m.ImagePath is not null).Take(FeaturedCount).ToList();
        var featuredIds = featured.Select(m => m.Id).ToHashSet();

        return new HomeFeed(
            featured,
            byHighlight.Where(m => !featuredIds.Contains(m.Id)).Take(RailCount).ToList(),
            ByRating(open.Where(m => m.Difficulty == DifficultyLevel.Easy)).Take(RailCount).ToList(),
            ByRating(open.Where(m => m.Difficulty == DifficultyLevel.Hard)).Take(RailCount).ToList(),
            IslandSummary(all));
    }

    // Cuma pulau yang punya gunung, urut sesuai enum Island. Gunung dengan provinsi gak dikenal gak dihitung.
    public static List<IslandCount> IslandSummary(IEnumerable<MountainListDto> all) =>
        all.Where(m => m.Island is not null)
            .GroupBy(m => m.Island!.Value)
            .OrderBy(g => g.Key)
            .Select(g => new IslandCount(g.Key, g.Count()))
            .ToList();

    public static List<MountainListDto> SameIsland(IReadOnlyList<MountainListDto> all, int mountainId,
        Island? island, int count)
    {
        if (island is null)
            return [];

        return ByRating(all.Where(m => m.Island == island && m.Id != mountainId)).Take(count).ToList();
    }

    // Foto buat panel kiri halaman akun. Null = gak ada gunung Open yang punya foto.
    public static MountainListDto? PickRandomPhoto(IReadOnlyList<MountainListDto> all, Random random)
    {
        var candidates = all.Where(m => m.Status == MountainStatus.Open && m.ImagePath is not null).ToList();
        return candidates.Count == 0 ? null : candidates[random.Next(candidates.Count)];
    }

    private static IOrderedEnumerable<MountainListDto> ByRating(IEnumerable<MountainListDto> mountains) =>
        mountains.OrderByDescending(m => m.AverageRating ?? 0).ThenBy(m => m.Name);
}
