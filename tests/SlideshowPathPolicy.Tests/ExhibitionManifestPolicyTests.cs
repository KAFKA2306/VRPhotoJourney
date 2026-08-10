using Xunit;

namespace SlideshowPathPolicy.Tests;

public sealed class ExhibitionManifestPolicyTests : IDisposable
{
    private readonly string projectRoot = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));

    public ExhibitionManifestPolicyTests()
    {
        Directory.CreateDirectory(Path.Combine(projectRoot, "Assets", "Photos"));
    }

    [Fact]
    public void AuditPreservesDisplayOrderAndDetectsDuplicateChecksum()
    {
        File.WriteAllText(Path.Combine(projectRoot, "Assets", "Photos", "a.jpg"), "same");
        File.WriteAllText(Path.Combine(projectRoot, "Assets", "Photos", "b.png"), "same");
        var manifest = Manifest(
            new global::YourNamespace.ExhibitionPhoto { id = "second", file = "b.png", order = 20 },
            new global::YourNamespace.ExhibitionPhoto { id = "first", file = "a.jpg", order = 10 });

        var result = global::YourNamespace.ExhibitionManifestPolicy.Audit(manifest, projectRoot);

        Assert.Equal(new[] { "first", "second" }, result.Select(item => item.Photo.id));
        Assert.Equal("READY_FOR_UNITY_IMPORT", result[0].ReasonCode);
        Assert.Equal("DUPLICATE_CHECKSUM", result[1].ReasonCode);
        Assert.Equal(result[0].ChecksumSha256, result[1].ChecksumSha256);
    }

    [Fact]
    public void AuditRejectsUnsupportedAndMissingWithoutInventingAssets()
    {
        File.WriteAllText(Path.Combine(projectRoot, "Assets", "Photos", "notes.txt"), "x");
        var manifest = Manifest(
            new global::YourNamespace.ExhibitionPhoto { id = "unsupported", file = "notes.txt", order = 0 },
            new global::YourNamespace.ExhibitionPhoto { id = "missing", file = "missing.png", order = 1 });

        var result = global::YourNamespace.ExhibitionManifestPolicy.Audit(manifest, projectRoot);

        Assert.Equal("UNSUPPORTED_FORMAT", result[0].ReasonCode);
        Assert.Equal("SOURCE_NOT_FOUND", result[1].ReasonCode);
        Assert.All(result, item => Assert.Equal("REJECTED", item.Status));
    }

    [Fact]
    public void AuditRejectsPathTraversal()
    {
        string outside = Path.Combine(projectRoot, "Assets", "secret.jpg");
        File.WriteAllText(outside, "private");
        var manifest = Manifest(new global::YourNamespace.ExhibitionPhoto { id = "escape", file = "../secret.jpg", order = 0 });

        var result = global::YourNamespace.ExhibitionManifestPolicy.Audit(manifest, projectRoot);

        Assert.Equal("PATH_OUTSIDE_SOURCE", result.Single().ReasonCode);
        Assert.Equal("REJECTED", result.Single().Status);
    }

    [Fact]
    public void AuditRequiresExplicitRightsStatus()
    {
        var manifest = Manifest(new global::YourNamespace.ExhibitionPhoto { id = "photo", file = "a.jpg", order = 0 });
        manifest.rightsStatus = "UNKNOWN";

        Assert.Throws<ArgumentException>(() => global::YourNamespace.ExhibitionManifestPolicy.Audit(manifest, projectRoot));
    }

    [Fact]
    public void AuditRejectsEmptyPackAndDuplicateOrder()
    {
        var empty = Manifest();
        Assert.Throws<ArgumentException>(() => global::YourNamespace.ExhibitionManifestPolicy.Audit(empty, projectRoot));

        var duplicateOrder = Manifest(
            new global::YourNamespace.ExhibitionPhoto { id = "a", file = "a.jpg", order = 1 },
            new global::YourNamespace.ExhibitionPhoto { id = "b", file = "b.jpg", order = 1 });
        Assert.Throws<ArgumentException>(() => global::YourNamespace.ExhibitionManifestPolicy.Audit(duplicateOrder, projectRoot));
    }

    [Fact]
    public void MarkdownReportContainsTraceableCountsAndChecksums()
    {
        string path = Path.Combine(projectRoot, "Assets", "Photos", "a.jpg");
        File.WriteAllText(path, "photo");
        var manifest = Manifest(new global::YourNamespace.ExhibitionPhoto { id = "photo-1", file = "a.jpg", order = 0 });
        var audits = global::YourNamespace.ExhibitionManifestPolicy.Audit(manifest, projectRoot);

        string report = global::YourNamespace.ExhibitionManifestPolicy.ToMarkdown(manifest, audits, "2026-08-10T00:00:00Z", "2022.3", "test");

        Assert.Contains("accepted_count: 1", report);
        Assert.Contains(global::YourNamespace.ExhibitionManifestPolicy.Sha256(path), report);
        Assert.Contains("SAMPLE_LICENSED", report);
    }

    private static global::YourNamespace.ExhibitionManifest Manifest(params global::YourNamespace.ExhibitionPhoto[] photos) => new()
    {
        schemaVersion = 1,
        exhibitionId = "sample-exhibition",
        title = "Sample Exhibition",
        organizer = "Sample Organizer",
        sourceFolder = "Assets/Photos",
        rightsStatus = global::YourNamespace.ExhibitionManifestPolicy.SampleLicensed,
        photos = photos
    };

    public void Dispose() => Directory.Delete(projectRoot, true);
}
