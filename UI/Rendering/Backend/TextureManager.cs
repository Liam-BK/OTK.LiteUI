using OpenTK.Graphics.OpenGL4;
using OTK.LiteUI.UI.Rendering;
using OTK.LiteUI.UI.Utilities;

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

public class TextureManager
{
    private TextureArray? textures = null;

    private int _maxTextureUnits = -1;

    public int TextureCapacity => textures?.Capacity ?? throw new MissingMemberException("TextureArray must be initialized before accessing it.");

    public TextureResolution GetTextureResolution => textures is not null ? textures.Resolution : TextureResolution.INVALID;

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

    public bool ContainsTexture(string name) => textures?.ContainsTexture(name) ?? throw new MissingMemberException("TextureArray must be initialized before accessing it.");

    public void CreateTextureArray(int capacity, TextureResolution resolution)
    {
        if (textures is not null) throw new InvalidOperationException("TextureArray has already been created.");
        textures = new TextureArray(capacity, resolution);
    }

    public void CreateTextureArray(TextureResolution resolution, int capacity)
    {
        if (textures is not null) throw new InvalidOperationException("TextureArray has already been created.");
        textures = new TextureArray(capacity, resolution);
    }

    public bool TryLoadTexture(string path, string name, TextureResolution? forceResolution = null, EmptyPixelType type = EmptyPixelType.Transparent, bool greyScale = false)
    {
        try
        {
            var data = ImageLoader.LoadImage(path, ImageLoader.Flip.Vertical, greyScale);
            data.ConvertToResolution(forceResolution is null ? InferResolution(data) : forceResolution.Value);
            if (textures is null) throw new InvalidOperationException("TextureArray must be created before loading textures.");
            textures.AddTexture(data, name);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[TextureManager] Failed to load texture at path: {path}");
            Console.WriteLine(ex);
            return false;
        }
    }

    public bool LoadAllTextures(string directory, EmptyPixelType type, TextureResolution? forceResolution = null, bool greyScale = false)
    {
        if (!Directory.Exists(directory))
            return false;

        foreach (var file in Directory.GetFiles(directory))
        {
            if (!Path.GetExtension(file).Equals(".png", StringComparison.CurrentCultureIgnoreCase))
                continue;

            string name = Path.GetFileNameWithoutExtension(file);
            TryLoadTexture(file, name, forceResolution, type, greyScale);
        }

        return true;
    }

    public void UploadTexture(ImageData data, string name)
    {
        if (textures is null)
            throw new InvalidOperationException("TextureArray must be created before uploading textures.");

        if (textures.LastLayer >= textures.Capacity)
            throw new InvalidOperationException("TextureArray capacity exceeded.");

        textures.AddTexture(data, name);
    }

    public bool TryGetTexture(string name, out int layer)
    {
        layer = textures?.GetTextureLayer(name) ?? -1;
        return layer >= 0;
    }

    public void Bind(int textureUnit)
    {
        if (_maxTextureUnits < 0) _maxTextureUnits = GL.GetInteger(GetPName.MaxTextureImageUnits);
        if (textureUnit < 0 || textureUnit >= _maxTextureUnits)
            throw new ArgumentOutOfRangeException(nameof(textureUnit));

        if (textures is null)
            throw new KeyNotFoundException($"Texture array not initialized.");

        textures.Bind(textureUnit);
    }

    public bool TryDeleteTextures()
    {
        if (textures is null) return false;
        textures.Dispose();
        textures = null;
        return true;
    }
}