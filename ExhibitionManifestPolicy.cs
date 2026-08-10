using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace YourNamespace
{
    [Serializable]
    public class ExhibitionManifest
    {
        public int schemaVersion = 1;
        public string exhibitionId = "";
        public string title = "";
        public string organizer = "";
        public string sourceFolder = "";
        public string rightsStatus = "";
        public ExhibitionPhoto[] photos = Array.Empty<ExhibitionPhoto>();
    }

    [Serializable]
    public class ExhibitionPhoto
    {
        public string id = "";
        public string file = "";
        public int order;
        public string title = "";
        public string caption = "";
        public string author = "";
    }

    public sealed class ExhibitionPhotoAudit
    {
        public ExhibitionPhoto Photo { get; set; } = new ExhibitionPhoto();
        public string SourcePath { get; set; } = "";
        public string ChecksumSha256 { get; set; } = "";
        public string Status { get; set; } = "REJECTED";
        public string ReasonCode { get; set; } = "UNVALIDATED";
    }

    public static class ExhibitionManifestPolicy
    {
        public const int SchemaVersion = 1;
        public const string ConfirmedByClient = "CONFIRMED_BY_CLIENT";
        public const string SampleLicensed = "SAMPLE_LICENSED";

        private static readonly HashSet<string> AllowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png"
        };

        public static IReadOnlyList<ExhibitionPhotoAudit> Audit(ExhibitionManifest manifest, string projectRoot)
        {
            if (manifest == null) throw new ArgumentNullException(nameof(manifest));
            if (manifest.schemaVersion != SchemaVersion) throw new ArgumentException("Unsupported exhibition manifest schemaVersion.");
            if (!IsStableId(manifest.exhibitionId)) throw new ArgumentException("exhibitionId must use lowercase letters, digits, '-' or '_'.");
            if (string.IsNullOrWhiteSpace(manifest.title)) throw new ArgumentException("title is required.");
            if (string.IsNullOrWhiteSpace(manifest.organizer)) throw new ArgumentException("organizer is required.");
            if (manifest.rightsStatus != ConfirmedByClient && manifest.rightsStatus != SampleLicensed)
                throw new ArgumentException("rightsStatus must be CONFIRMED_BY_CLIENT or SAMPLE_LICENSED.");
            if (manifest.photos == null || manifest.photos.Length == 0) throw new ArgumentException("At least one photo is required.");

            string root = Path.GetFullPath(projectRoot);
            string sourceRoot = ResolveInsideProject(root, manifest.sourceFolder, "sourceFolder");
            if (!Directory.Exists(sourceRoot)) throw new DirectoryNotFoundException($"Source folder does not exist: {sourceRoot}");

            var ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var orders = new HashSet<int>();
            var checksums = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var results = new List<ExhibitionPhotoAudit>();

            foreach (ExhibitionPhoto photo in manifest.photos.OrderBy(item => item.order).ThenBy(item => item.id, StringComparer.Ordinal))
            {
                if (photo == null) throw new ArgumentException("Photo entries must not be null.");
                if (!IsStableId(photo.id)) throw new ArgumentException($"Invalid photo id: {photo.id}");
                if (!ids.Add(photo.id)) throw new ArgumentException($"Duplicate photo id: {photo.id}");
                if (photo.order < 0 || !orders.Add(photo.order)) throw new ArgumentException($"Invalid or duplicate display order: {photo.order}");
                if (string.IsNullOrWhiteSpace(photo.file)) throw new ArgumentException($"Photo {photo.id} file is required.");

                var audit = new ExhibitionPhotoAudit { Photo = photo };
                string extension = Path.GetExtension(photo.file);
                if (!AllowedExtensions.Contains(extension))
                {
                    audit.ReasonCode = "UNSUPPORTED_FORMAT";
                    results.Add(audit);
                    continue;
                }

                string sourcePath;
                try
                {
                    sourcePath = ResolveInsideDirectory(sourceRoot, photo.file);
                }
                catch (ArgumentException)
                {
                    audit.ReasonCode = "PATH_OUTSIDE_SOURCE";
                    results.Add(audit);
                    continue;
                }
                audit.SourcePath = sourcePath;

                if (!File.Exists(sourcePath))
                {
                    audit.ReasonCode = "SOURCE_NOT_FOUND";
                    results.Add(audit);
                    continue;
                }

                string checksum = Sha256(sourcePath);
                audit.ChecksumSha256 = checksum;
                if (!checksums.Add(checksum))
                {
                    audit.ReasonCode = "DUPLICATE_CHECKSUM";
                    results.Add(audit);
                    continue;
                }

                audit.Status = "ACCEPTED";
                audit.ReasonCode = "READY_FOR_UNITY_IMPORT";
                results.Add(audit);
            }

            return results;
        }

        public static string ProjectRelativePath(string projectRoot, string fullPath)
        {
            string root = Path.GetFullPath(projectRoot).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string path = Path.GetFullPath(fullPath);
            if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Path is outside the Unity project.");
            return path.Substring(root.Length).Replace('\\', '/');
        }

        public static string Sha256(string path)
        {
            using (var stream = File.OpenRead(path))
            using (var sha = SHA256.Create())
                return ToHex(sha.ComputeHash(stream));
        }

        public static string ToMarkdown(ExhibitionManifest manifest, IEnumerable<ExhibitionPhotoAudit> audits, string generatedAtUtc, string unityVersion, string toolRevision)
        {
            var rows = audits.ToList();
            var builder = new StringBuilder();
            builder.AppendLine($"# Exhibition Report — {manifest.title}");
            builder.AppendLine();
            builder.AppendLine($"- exhibition_id: `{manifest.exhibitionId}`");
            builder.AppendLine($"- organizer: {manifest.organizer}");
            builder.AppendLine($"- rights_status: `{manifest.rightsStatus}`");
            builder.AppendLine($"- generated_at_utc: `{generatedAtUtc}`");
            builder.AppendLine($"- unity_version: `{unityVersion}`");
            builder.AppendLine($"- tool_revision: `{toolRevision}`");
            builder.AppendLine($"- input_count: {rows.Count}");
            builder.AppendLine($"- accepted_count: {rows.Count(item => item.Status == "ACCEPTED")}");
            builder.AppendLine($"- rejected_count: {rows.Count(item => item.Status != "ACCEPTED")}");
            builder.AppendLine();
            builder.AppendLine("| order | photo_id | status | reason | sha256 |");
            builder.AppendLine("| ---: | --- | --- | --- | --- |");
            foreach (var item in rows.OrderBy(row => row.Photo.order))
                builder.AppendLine($"| {item.Photo.order} | {Escape(item.Photo.id)} | {item.Status} | {item.ReasonCode} | {item.ChecksumSha256} |");
            return builder.ToString();
        }

        private static bool IsStableId(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            return value.All(ch => (ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == '-' || ch == '_');
        }

        private static string ResolveInsideProject(string projectRoot, string relativePath, string field)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) throw new ArgumentException($"{field} is required.");
            string full = Path.GetFullPath(Path.Combine(projectRoot, relativePath));
            string root = projectRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException($"{field} must stay inside the Unity project.");
            return full;
        }

        private static string ResolveInsideDirectory(string directory, string relativePath)
        {
            string root = directory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
            string full = Path.GetFullPath(Path.Combine(directory, relativePath));
            if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Photo path escapes sourceFolder.");
            return full;
        }

        private static string ToHex(byte[] bytes)
        {
            var builder = new StringBuilder(bytes.Length * 2);
            foreach (byte value in bytes) builder.Append(value.ToString("x2"));
            return builder.ToString();
        }

        private static string Escape(string value) => (value ?? "").Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
    }
}
