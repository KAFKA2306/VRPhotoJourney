using Xunit;

namespace SlideshowPathPolicy.Tests;

public sealed class SlideshowPathPolicyTests : IDisposable
{
    private readonly string directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

    public SlideshowPathPolicyTests() => Directory.CreateDirectory(directory);

    [Fact]
    public void EnumerateImagesFiltersAndSortsSupportedFiles()
    {
        File.WriteAllText(Path.Combine(directory, "b.PNG"), "x");
        File.WriteAllText(Path.Combine(directory, "a.jpg"), "x");
        File.WriteAllText(Path.Combine(directory, "ignored.txt"), "x");

        string[] result = global::YourNamespace.SlideshowPathPolicy.EnumerateImages(directory);

        Assert.Equal(new[] { "a.jpg", "b.PNG" }, result.Select(Path.GetFileName));
    }

    [Fact]
    public void EnumerateImagesRejectsMissingDirectory()
    {
        Assert.Throws<DirectoryNotFoundException>(() =>
            global::YourNamespace.SlideshowPathPolicy.EnumerateImages(Path.Combine(directory, "missing")));
    }

    [Theory]
    [InlineData(0, 1, 3, 1)]
    [InlineData(2, 1, 3, 0)]
    [InlineData(0, -1, 3, 2)]
    [InlineData(0, 1, 0, -1)]
    public void WrapIndexHandlesBoundaries(int current, int delta, int count, int expected)
    {
        Assert.Equal(expected, global::YourNamespace.SlideshowPathPolicy.WrapIndex(current, delta, count));
    }

    public void Dispose() => Directory.Delete(directory, true);
}
