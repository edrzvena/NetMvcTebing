using Tebing.Common.Regions;

namespace Tebing.Tests;

public class IslandRegionTests
{
    [Theory]
    [InlineData("Jawa Barat", Island.Jawa)]
    [InlineData(" jawa barat ", Island.Jawa)]
    [InlineData("DKI Jakarta", Island.Jawa)]
    [InlineData("DI Yogyakarta", Island.Jawa)]
    [InlineData("Banten", Island.Jawa)]
    [InlineData("Aceh", Island.Sumatra)]
    [InlineData("Sumatera Barat", Island.Sumatra)]
    [InlineData("Kepulauan Bangka Belitung", Island.Sumatra)]
    [InlineData("Bali", Island.BaliNusaTenggara)]
    [InlineData("Nusa Tenggara Timur", Island.BaliNusaTenggara)]
    [InlineData("Kalimantan Timur", Island.Kalimantan)]
    [InlineData("Gorontalo", Island.Sulawesi)]
    [InlineData("Sulawesi Utara", Island.Sulawesi)]
    [InlineData("Maluku Utara", Island.MalukuPapua)]
    [InlineData("Papua Pegunungan", Island.MalukuPapua)]
    public void FromProvince_MapsKnownProvinces(string province, Island expected) =>
        Assert.Equal(expected, IslandRegion.FromProvince(province));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Provinsi Antah")]
    public void FromProvince_UnknownIsNull(string? province) =>
        Assert.Null(IslandRegion.FromProvince(province));

    [Fact]
    public void ToLabel_UsesIndonesianNames() =>
        Assert.Equal("Bali & Nusa Tenggara", Island.BaliNusaTenggara.ToLabel());
}
