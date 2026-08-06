using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace YourNamespace
{
    public static class SlideshowPathPolicy
    {
        private static readonly HashSet<string> AllowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png"
        };

        public static string[] EnumerateImages(string folderPath)
        {
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                throw new ArgumentException("Photo folder path is empty.", nameof(folderPath));
            }

            string fullPath = Path.GetFullPath(folderPath.Trim());
            if (!Directory.Exists(fullPath))
            {
                throw new DirectoryNotFoundException($"Photo folder does not exist: {fullPath}");
            }

            try
            {
                return Directory.EnumerateFiles(fullPath, "*", SearchOption.TopDirectoryOnly)
                    .Where(path => AllowedExtensions.Contains(Path.GetExtension(path)))
                    .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                    .ToArray();
            }
            catch (UnauthorizedAccessException exception)
            {
                throw new IOException($"Photo folder is not readable: {fullPath}", exception);
            }
        }

        public static int WrapIndex(int currentIndex, int delta, int itemCount)
        {
            if (itemCount <= 0)
            {
                return -1;
            }

            int normalized = currentIndex < 0 ? 0 : currentIndex % itemCount;
            return (normalized + delta % itemCount + itemCount) % itemCount;
        }

        public static bool IsValidIndex(int index, int itemCount)
        {
            return itemCount > 0 && index >= 0 && index < itemCount;
        }
    }
}
