using Tebing.Common.Enums;
using Tebing.Common.Feeds;
using Tebing.Common.Regions;
using Tebing.DTOs.Mountain;

namespace Tebing.Tests;

public class MountainFeedTests
{
    private static MountainListDto M(int id, double? rating = null, int reviews = 0, int height = 2000,
        DifficultyLevel difficulty = DifficultyLevel.Medium, bool open = true, bool photo = true,
        Island? island = Island.Jawa, string? name = null) => new()
    {
        Id = id,
        Name = name ?? $"Gunung {id:D2}",
        AverageRating = rating,
        ReviewCount = reviews,
        HeightMeters = height,
        Difficulty = difficulty,
        Status = open ? MountainStatus.Open : MountainStatus.Closed,
        ImagePath = photo ? $"/images/mountains/{id}" : null,
        Island = island
    };

    private static List<int> Ids(IEnumerable<MountainListDto> list) => list.Select(m => m.Id).ToList();

    [Fact]
    public void Featured_SkipsMountainsWithoutPhoto()
    {
        var feed = MountainFeed.BuildHome([M(1, photo: false, rating: 5), M(2)]);
        Assert.Equal([2], Ids(feed.Featured));
    }

    [Fact]
    public void Featured_SkipsClosedMountains()
    {
        var feed = MountainFeed.BuildHome([M(1, open: false, rating: 5), M(2)]);
        Assert.Equal([2], Ids(feed.Featured));
    }

    [Fact]
    public void Featured_OrdersByRatingThenReviewsThenHeight()
    {
        var feed = MountainFeed.BuildHome([
            M(1, height: 3000),
            M(2, height: 3700),
            M(3, rating: 4.5, reviews: 2),
            M(4, rating: 4.5, reviews: 9),
            M(5, rating: 4.9, reviews: 1)
        ]);
        Assert.Equal([5, 4, 3, 2, 1], Ids(feed.Featured));
    }

    [Fact]
    public void Featured_TakesAtMostFive()
    {
        var feed = MountainFeed.BuildHome(Enumerable.Range(1, 8).Select(i => M(i)).ToList());
        Assert.Equal(5, feed.Featured.Count);
    }

    [Fact]
    public void Featured_WithFewerEligible_ReturnsWhatExists()
    {
        var feed = MountainFeed.BuildHome([M(1), M(2), M(3, photo: false)]);
        Assert.Equal(2, feed.Featured.Count);
    }

    [Fact]
    public void Picks_ExcludesFeatured_AndTakesAtMostTen()
    {
        var all = Enumerable.Range(1, 20).Select(i => M(i, height: 4000 - i)).ToList();
        var feed = MountainFeed.BuildHome(all);
        Assert.Equal([1, 2, 3, 4, 5], Ids(feed.Featured));
        Assert.Equal(Enumerable.Range(6, 10).ToList(), Ids(feed.Picks));
    }

    [Fact]
    public void Picks_IncludesOpenMountainsWithoutPhoto()
    {
        var feed = MountainFeed.BuildHome([M(1), M(2, photo: false)]);
        Assert.Equal([2], Ids(feed.Picks));
    }

    [Fact]
    public void Beginner_OnlyEasyAndOpen()
    {
        var feed = MountainFeed.BuildHome([
            M(1, difficulty: DifficultyLevel.Easy, name: "B"),
            M(2, difficulty: DifficultyLevel.Easy, open: false),
            M(3, difficulty: DifficultyLevel.Hard),
            M(4, difficulty: DifficultyLevel.Easy, name: "A")
        ]);
        Assert.Equal([4, 1], Ids(feed.Beginner));
    }

    [Fact]
    public void Challenging_OnlyHardAndOpen()
    {
        var feed = MountainFeed.BuildHome([
            M(1, difficulty: DifficultyLevel.Hard),
            M(2, difficulty: DifficultyLevel.Hard, open: false),
            M(3, difficulty: DifficultyLevel.Easy)
        ]);
        Assert.Equal([1], Ids(feed.Challenging));
    }

    [Fact]
    public void BuildHome_EmptyInput_AllListsEmpty()
    {
        var feed = MountainFeed.BuildHome([]);
        Assert.Empty(feed.Featured);
        Assert.Empty(feed.Picks);
        Assert.Empty(feed.Beginner);
        Assert.Empty(feed.Challenging);
        Assert.Empty(feed.Islands);
    }

    [Fact]
    public void IslandSummary_CountsInEnumOrder_SkipsEmptyAndNullIsland()
    {
        var summary = MountainFeed.IslandSummary([
            M(1, island: Island.Sumatra), M(2, island: Island.Jawa), M(3, island: Island.Jawa), M(4, island: null)
        ]);
        Assert.Equal([new IslandCount(Island.Jawa, 2), new IslandCount(Island.Sumatra, 1)], summary);
    }

    [Fact]
    public void SameIsland_ExcludesSelf_AndOtherIslands()
    {
        var result = MountainFeed.SameIsland([
            M(1, island: Island.Jawa), M(2, island: Island.Jawa, rating: 4), M(3, island: Island.BaliNusaTenggara), M(4, island: Island.Jawa)
        ], mountainId: 1, Island.Jawa, count: 10);
        Assert.Equal([2, 4], Ids(result));
    }

    [Fact]
    public void SameIsland_NullIsland_ReturnsEmpty() =>
        Assert.Empty(MountainFeed.SameIsland([M(1, island: null), M(2, island: null)], 1, null, 10));

    [Fact]
    public void PickRandomPhoto_OnlyOpenWithPhoto()
    {
        List<MountainListDto> all = [M(1), M(2, photo: false), M(3, open: false), M(4)];
        var random = new Random(42);
        for (var i = 0; i < 50; i++)
        {
            var picked = MountainFeed.PickRandomPhoto(all, random);
            Assert.NotNull(picked);
            Assert.Contains(picked.Id, new[] { 1, 4 });
        }
    }

    [Fact]
    public void SiteStats_CountsMountainsRegionsAndActiveRoutes()
    {
        var a = M(1, island: Island.Jawa); a.RouteCount = 3;
        var b = M(2, island: Island.Sumatra, open: false); b.RouteCount = 2;
        var c = M(3, island: null); c.RouteCount = 1;
        Assert.Equal(new SiteStats(3, 2, 6), MountainFeed.SiteStats([a, b, c]));
    }

    [Fact]
    public void SiteStats_Empty_AllZero() => Assert.Equal(new SiteStats(0, 0, 0), MountainFeed.SiteStats([]));

    [Fact]
    public void PickRandomPhoto_NoEligible_ReturnsNull() =>
        Assert.Null(MountainFeed.PickRandomPhoto([M(1, photo: false), M(2, open: false)], new Random(1)));
}
