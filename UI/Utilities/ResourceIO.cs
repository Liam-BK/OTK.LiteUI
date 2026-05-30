using System.Reflection;

namespace OTK.LiteUI.UI.Utilities
{
    public static class ResourceIO
    {
        public static Stream? GetLoadingStream(string pathOrResource)
        {
            if (File.Exists(pathOrResource))
                return File.OpenRead(pathOrResource);

            string resourceName = pathOrResource
                .Replace('\\', '.')
                .Replace('/', '.');

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (string manifestName in assembly.GetManifestResourceNames())
                {
                    // Console.WriteLine($"found file names: {manifestName}");
                    if (manifestName.EndsWith(resourceName,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        return assembly.GetManifestResourceStream(manifestName);
                    }
                }
            }

            return null;
        }

        public static Stream? GetSavingStream(string filePath)
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