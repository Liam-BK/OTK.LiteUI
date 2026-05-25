using OpenTK.Graphics.OpenGL4;

public enum TextureResolution
{
    INVALID = -1,
    R16,
    R32,
    R64,
    R128,
    R256,
    R512,
    R1024,
    R2048,
    R4096
}
public static class TextureManager
{
    public static Dictionary<string, Dictionary<TextureResolution, int>> TextureReferences
        = new();
    private static readonly Dictionary<TextureResolution, int> _arrays = new();

    private static readonly Dictionary<TextureResolution, int> _nextLayer = new();

    private static readonly List<string> _textureNames = new();

    private static readonly TextureUnit[] _units =
        Enumerable.Range(0, 32)
            .Select(i => TextureUnit.Texture0 + i)
            .ToArray();

    public static int TextureCount => _textureNames.Count;

    public static int FindResolution(TextureResolution resolution)
    {
        return resolution switch
        {
            TextureResolution.R16 => 16,
            TextureResolution.R32 => 32,
            TextureResolution.R64 => 64,
            TextureResolution.R128 => 128,
            TextureResolution.R256 => 256,
            TextureResolution.R512 => 512,
            TextureResolution.R1024 => 1024,
            TextureResolution.R2048 => 2048,
            TextureResolution.R4096 => 4096,
            _ => throw new ArgumentOutOfRangeException(nameof(resolution))
        };
    }

    public static void CreateTextureArray(int maxLayers)
    {
        int handle = GL.GenTexture();

        GL.BindTexture(TextureTarget.Texture2DArray, handle);

        var res = UIScene.Resolution;

        GL.TexImage3D(
            TextureTarget.Texture2DArray,
            0,
            PixelInternalFormat.Rgba,
            FindResolution(res),
            FindResolution(res),
            maxLayers,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            IntPtr.Zero
        );

        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
        GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

        _arrays[res] = handle;
        _nextLayer[res] = 0;
    }

    public static bool TryLoadTexture(string path, string name, out TextureResolution finalResolution, TextureResolution? forceResolution = null, EmptyPixelType type = EmptyPixelType.Transparent, bool greyScale = false)
    {
        try
        {
            var data = ImageLoader.LoadImage(path, ImageLoader.Flip.Vertical, greyScale);
            if (forceResolution is null) finalResolution = InferResolution(data);
            else finalResolution = (TextureResolution)forceResolution;
            data.ConvertToResolution(finalResolution, type);
            UploadTexture(data, name);
            return true;
        }
        catch
        {
            finalResolution = TextureResolution.INVALID;
            return false;
        }
    }

    public static bool LoadAllTextures(string directory, EmptyPixelType type, TextureResolution? forceResolution = null, bool greyScale = false)
    {
        if (!Directory.Exists(directory))
            return false;

        foreach (var file in Directory.GetFiles(directory))
        {
            if (Path.GetExtension(file).ToLower() != ".png")
                continue;

            string name = Path.GetFileNameWithoutExtension(file);
            TryLoadTexture(file, name, out var result, forceResolution, type, greyScale);
        }

        return true;
    }

    public static void UploadTexture(ImageData data, string name)
    {
        TextureResolution res = InferResolution(data);

        if (!_arrays.ContainsKey(res))
            throw new Exception($"TextureResolution {res} not initialized.");

        int layer = AllocateLayer(res);

        int array = _arrays[res];

        GL.BindTexture(TextureTarget.Texture2DArray, array);

        GL.TexSubImage3D(
            TextureTarget.Texture2DArray,
            0,
            0,
            0,
            layer,
            FindResolution(res),
            FindResolution(res),
            1,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            data.Pixels
        );

        if (!TextureReferences.TryGetValue(name, out var set))
        {
            set = new Dictionary<TextureResolution, int>();
            TextureReferences[name] = set;
            _textureNames.Add(name);
        }

        set[res] = layer;
    }

    private static int AllocateLayer(TextureResolution res)
    {
        int layer = _nextLayer[res]++;
        return layer;
    }

    private static TextureResolution InferResolution(ImageData data)
    {
        int max = Math.Max(data.Width, data.Height);

        if (max <= 16) return TextureResolution.R16;
        if (max <= 32) return TextureResolution.R32;
        if (max <= 64) return TextureResolution.R64;
        if (max <= 128) return TextureResolution.R128;
        if (max <= 256) return TextureResolution.R256;
        if (max <= 512) return TextureResolution.R512;
        if (max <= 1024) return TextureResolution.R1024;
        if (max <= 2048) return TextureResolution.R2048;

        return TextureResolution.R4096;
    }

    public static bool TryGetTexture(int index, TextureResolution res, out int layer)
    {
        layer = -1;

        if (index < 0 || index >= _textureNames.Count)
            return false;

        string name = _textureNames[index];

        return TryGetTexture(name, res, out layer);
    }

    public static bool TryGetTexture(string name, TextureResolution res, out int layer)
    {
        layer = -1;

        if (!TextureReferences.TryGetValue(name, out var set))
            return false;

        return set.TryGetValue(res, out layer);
    }

    public static void Bind(TextureResolution res, int textureUnit)
    {
        if (textureUnit < 0 || textureUnit >= _units.Length)
            throw new ArgumentOutOfRangeException(nameof(textureUnit));

        if (!_arrays.TryGetValue(res, out int handle))
            throw new KeyNotFoundException($"Texture array for {res} not initialized.");

        GL.ActiveTexture(_units[textureUnit]);
        GL.BindTexture(TextureTarget.Texture2DArray, _arrays[res]);
    }

    public static void Unbind(int textureUnit)
    {
        GL.ActiveTexture(_units[textureUnit]);
        GL.BindTexture(TextureTarget.Texture2DArray, 0);
    }

    private static void ClearLayer(TextureResolution res, int layer)
    {
        if (!_arrays.TryGetValue(res, out int handle))
            return;

        GL.BindTexture(TextureTarget.Texture2DArray, handle);

        // zero-fill layer (optional but useful for debugging)
        byte[] empty = new byte[4 * 1 * 1]; // RGBA 1x1

        GL.TexSubImage3D(
            TextureTarget.Texture2DArray,
            0,
            0,
            0,
            layer,
            1,
            1,
            1,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            empty
        );
    }

    public static bool TryDelete(string name)
    {
        if (!TextureReferences.TryGetValue(name, out var set))
            return false;

        foreach (var kv in set)
        {
            TextureResolution res = kv.Key;
            int layer = kv.Value;

            // optional GPU clear (not strictly required, but good hygiene)
            ClearLayer(res, layer);
        }

        TextureReferences.Remove(name);
        _textureNames.Remove(name);

        return true;
    }

    public static bool TryDelete(string name, TextureResolution res)
    {
        if (!TextureReferences.TryGetValue(name, out var set))
            return false;

        if (!set.TryGetValue(res, out int layer))
            return false;

        set.Remove(res);

        ClearLayer(res, layer);

        return true;
    }

    public static void DeleteAll()
    {
        TextureReferences.Clear();
        _textureNames.Clear();

        foreach (var array in _arrays.Values)
        {
            GL.DeleteTexture(array);
        }

        _arrays.Clear();
        _nextLayer.Clear();
    }
}