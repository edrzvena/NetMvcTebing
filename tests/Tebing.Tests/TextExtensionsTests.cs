using Tebing.Common.Text;

namespace Tebing.Tests;

public class TextExtensionsTests
{
    [Theory]
    [InlineData("Danau di kawah. Sabana luas.", "Danau di kawah.")]
    [InlineData("Tanpa titik sama sekali", "Tanpa titik sama sekali")]
    [InlineData("Tingginya 2.958 mdpl dan dingin. Bawa jaket.", "Tingginya 2.958 mdpl dan dingin.")]
    [InlineData("  Spasi di depan. Lanjut.", "Spasi di depan.")]
    public void FirstSentence_TakesFirstSentence(string input, string expected) =>
        Assert.Equal(expected, input.FirstSentence());

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FirstSentence_BlankIsNull(string? input) => Assert.Null(input.FirstSentence());
}
