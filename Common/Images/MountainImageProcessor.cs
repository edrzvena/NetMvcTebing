using SkiaSharp;

namespace Tebing.Common.Images;

// Foto gunung selalu disimpan sebagai 2 file WebP 16:9: "-640" buat kartu di daftar, "-1200" buat halaman
// detail. File asli dari kamera (bisa 3-5 MB) gak pernah dikirim ke browser — cuma hasil resize ini.
// Sengaja gak bergantung ke ASP.NET, biar bisa dipakai juga buat nyiapin foto seed di luar app.
public static class MountainImageProcessor
{
    public static readonly int[] Widths = [640, 1200];

    // URL file per ukuran dari path dasar yang disimpan di Mountain.ImagePath.
    public static string Url(string imagePath, int width) => $"{imagePath}-{width}.webp";

    private const int AspectWidth = 16;
    private const int AspectHeight = 9;
    private const int Quality = 78;

    // Decode cukup sebesar ini (sisi pendek) — hemat memori buat foto kamera 6000x4000 tanpa ngurangin
    // kualitas output 1200px.
    private const int DecodeMinShortEdge = 1500;

    // Hasil per lebar (640/1200) → isi file WebP. Null kalau stream bukan gambar yang bisa dibaca.
    public static Dictionary<int, byte[]>? Process(Stream input)
    {
        using var codec = SKCodec.Create(input);
        if (codec is null)
            return null;

        using var decoded = Decode(codec);
        if (decoded is null)
            return null;

        using var oriented = ApplyOrigin(decoded, codec.EncodedOrigin);
        using var image = SKImage.FromBitmap(oriented);

        var crop = CenterCrop(image.Width, image.Height);
        var result = new Dictionary<int, byte[]>();
        foreach (var width in Widths)
        {
            // Gak di-upscale: foto kecil tetap kecil (rasio tetap 16:9).
            var outWidth = Math.Min(width, (int)crop.Width);
            var outHeight = outWidth * AspectHeight / AspectWidth;

            using var surface = SKSurface.Create(new SKImageInfo(outWidth, outHeight));
            surface.Canvas.DrawImage(image, crop, new SKRect(0, 0, outWidth, outHeight),
                new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear));

            using var snapshot = surface.Snapshot();
            using var data = snapshot.Encode(SKEncodedImageFormat.Webp, Quality);
            result[width] = data.ToArray();
        }

        return result;
    }

    private static SKBitmap? Decode(SKCodec codec)
    {
        var shortEdge = Math.Min(codec.Info.Width, codec.Info.Height);
        var scale = Math.Min(1f, (float)DecodeMinShortEdge / shortEdge);
        var size = scale < 1f ? codec.GetScaledDimensions(scale) : codec.Info.Size;

        var info = new SKImageInfo(size.Width, size.Height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
        return SKBitmap.Decode(codec, info);
    }

    // Potong tengah ke 16:9.
    private static SKRect CenterCrop(int width, int height)
    {
        if (width * AspectHeight > height * AspectWidth)
        {
            var cropWidth = height * AspectWidth / (float)AspectHeight;
            var left = (width - cropWidth) / 2;
            return new SKRect(left, 0, left + cropWidth, height);
        }

        var cropHeight = width * AspectHeight / (float)AspectWidth;
        var top = (height - cropHeight) / 2;
        return new SKRect(0, top, width, top + cropHeight);
    }

    // Foto HP sering disimpan miring + tag EXIF orientation; diputar dulu biar gak tampil miring.
    private static SKBitmap ApplyOrigin(SKBitmap source, SKEncodedOrigin origin)
    {
        float w = source.Width, h = source.Height;
        // x' = ScaleX*x + SkewX*y + TransX, y' = SkewY*x + ScaleY*y + TransY
        SKMatrix? matrix = origin switch
        {
            SKEncodedOrigin.TopRight => new SKMatrix(-1, 0, w, 0, 1, 0, 0, 0, 1),
            SKEncodedOrigin.BottomRight => new SKMatrix(-1, 0, w, 0, -1, h, 0, 0, 1),
            SKEncodedOrigin.BottomLeft => new SKMatrix(1, 0, 0, 0, -1, h, 0, 0, 1),
            SKEncodedOrigin.LeftTop => new SKMatrix(0, 1, 0, 1, 0, 0, 0, 0, 1),
            SKEncodedOrigin.RightTop => new SKMatrix(0, -1, h, 1, 0, 0, 0, 0, 1),
            SKEncodedOrigin.RightBottom => new SKMatrix(0, -1, h, -1, 0, w, 0, 0, 1),
            SKEncodedOrigin.LeftBottom => new SKMatrix(0, 1, 0, -1, 0, w, 0, 0, 1),
            _ => null
        };
        if (matrix is null)
            return source.Copy();

        var swap = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop
            or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
        var result = new SKBitmap(swap ? source.Height : source.Width, swap ? source.Width : source.Height);
        using var canvas = new SKCanvas(result);
        canvas.SetMatrix(matrix.Value);
        canvas.DrawBitmap(source, 0, 0);
        return result;
    }
}
