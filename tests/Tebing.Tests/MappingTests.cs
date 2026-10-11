using Tebing.Common.Regions;
using Tebing.Mappings;
using Tebing.Models;

namespace Tebing.Tests;

public class MappingTests
{
    private static Mountain Gede(string province = "Jawa Barat") => new()
    {
        Id = 1,
        Name = "Gunung Gede",
        Province = province,
        Description = "A. B.",
        ImagePath = "/images/mountains/gunung-gede",
        ImageCredit = "Afrogindahood / CC BY-SA 3.0 / Wikimedia Commons"
    };

    private static Booking Booking() => new()
    {
        Id = 7,
        User = new User { Id = 3, FullName = "Pendaki", Email = "p@x.test" },
        Mountain = Gede(),
        HikingRoute = new HikingRoute { Id = 2, Name = "Via Cibodas" }
    };

    [Fact]
    public void MountainToListDto_SetsIslandAndTagline()
    {
        var dto = Gede().ToListDto();
        Assert.Equal(Island.Jawa, dto.Island);
        Assert.Equal("A.", dto.Tagline);
    }

    [Fact]
    public void MountainToListDto_CopiesImageCredit() =>
        Assert.Equal("Afrogindahood / CC BY-SA 3.0 / Wikimedia Commons", Gede().ToListDto().ImageCredit);

    [Fact]
    public void MountainToListDto_UnknownProvince_IslandNull() =>
        Assert.Null(Gede("Provinsi Antah").ToListDto().Island);

    [Fact]
    public void MountainToDetailDto_SetsIsland() => Assert.Equal(Island.Jawa, Gede().ToDetailDto().Island);

    [Fact]
    public void BookingToListDto_CopiesMountainImagePath() =>
        Assert.Equal("/images/mountains/gunung-gede", Booking().ToListDto().MountainImagePath);

    [Fact]
    public void BookingToDetailDto_CopiesMountainImagePath() =>
        Assert.Equal("/images/mountains/gunung-gede", Booking().ToDetailDto().MountainImagePath);
}
