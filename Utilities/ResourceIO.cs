internal static class ResourceIO
{
    internal static Stream? GetLoadingStream(string pathOrResource)
    {
        // 1. Check file system first
        if (File.Exists(pathOrResource))
        {
            return File.OpenRead(pathOrResource);
        }

        // 2. Normalize resource name for matching
        string normalized = pathOrResource
            .Replace("\\", ".")
            .Replace("/", ".")
            .ToLowerInvariant();

        // 3. Search *all loaded assemblies*
        foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
        {
            string? match = asm
                .GetManifestResourceNames()
                .FirstOrDefault(n =>
                    n.ToLowerInvariant().EndsWith(normalized));

            if (match != null)
            {
                return asm.GetManifestResourceStream(match);
            }
        }

        return null; // Nothing matched anywhere
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