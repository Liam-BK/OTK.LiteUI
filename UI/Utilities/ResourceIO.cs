namespace OTK.LiteUI.UI.Utilities
{
    public static class ResourceIO
    {
        internal static Stream? GetLoadingStream(string pathOrResource)
        {
            if (File.Exists(pathOrResource))
                return File.OpenRead(pathOrResource);

            // 2. Convert to embedded resource name
            string resourceName = "OTK.LiteUI." + pathOrResource.Replace('\\', '.').Replace('/', '.');

            // 3. Assembly lookup (no scanning)
            return typeof(ResourceIO)
                .Assembly
                .GetManifestResourceStream(resourceName);
        }

        internal static Stream? GetSavingStream(string filePath)
        {
            try
            {
                string? directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                return File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            }
            catch
            {
                return null;
            }
        }

        internal static byte[]? GetBytes(string pathOrResource)
        {
            using Stream? stream = GetLoadingStream(pathOrResource);
            if (stream == null) return null;

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }
    }
}