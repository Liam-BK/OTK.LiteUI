using OpenTK.Graphics.OpenGL4;
using OTK.LiteUI.UI.Rendering;
using OTK.LiteUI.UI.Utilities;

/// <summary>
/// Specifies the dimensions of a texture layer.
/// </summary>
public enum TextureResolution
{
    /// <summary>
    /// Represents an invalid or unspecified resolution.
    /// </summary>
    INVALID = -1,

    /// <summary>16 × 16 pixels.</summary>
    R16,

    /// <summary>32 × 32 pixels.</summary>
    R32,

    /// <summary>64 × 64 pixels.</summary>
    R64,

    /// <summary>128 × 128 pixels.</summary>
    R128,

    /// <summary>256 × 256 pixels.</summary>
    R256,

    /// <summary>512 × 512 pixels.</summary>
    R512,

    /// <summary>1024 × 1024 pixels.</summary>
    R1024,

    /// <summary>2048 × 2048 pixels.</summary>
    R2048,

    /// <summary>4096 × 4096 pixels.</summary>
    R4096
}

namespace OTK.LiteUI.UI.Rendering.Backend
{
    /// <summary>
    /// Manages a texture array, including texture loading, uploading, lookup,
    /// binding, and disposal.
    /// </summary>
    public class TextureManager
    {
        private TextureArray? textures = null;

        private int _maxTextureUnits = -1;

        /// <summary>
        /// Gets the maximum number of texture layers the array can contain.
        /// </summary>
        /// <exception cref="MissingMemberException">
        /// Thrown if the texture array has not been initialized.
        /// </exception>
        public int TextureCapacity => textures?.Capacity ?? throw new MissingMemberException("TextureArray must be initialized before accessing it.");

        /// <summary>
        /// Gets the index of the next available texture layer.
        /// </summary>
        /// <exception cref="MissingMemberException">
        /// Thrown if the texture array has not been initialized.
        /// </exception>
        public int TextureCount => textures?.LastLayer ?? throw new MissingMemberException("TextureArray must be initialized before accessing it.");

        /// <summary>
        /// Gets the resolution assigned to the texture array, or
        /// <see cref="TextureResolution.INVALID"/> if it has not been initialized.
        /// </summary>
        public TextureResolution GetTextureResolution => textures is not null ? textures.Resolution : TextureResolution.INVALID;

        /// <summary>
        /// Converts a texture resolution value to its corresponding pixel dimension.
        /// </summary>
        /// <param name="resolution">The resolution to convert.</param>
        /// <returns>The width and height, in pixels, represented by the resolution.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the resolution is invalid.
        /// </exception>
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

        /// <summary>
        /// Determines whether the texture array contains a texture with the specified name.
        /// </summary>
        /// <param name="name">The texture name to search for.</param>
        /// <returns><see langword="true"/> if the texture exists; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="MissingMemberException">
        /// Thrown if the texture array has not been initialized.
        /// </exception>
        public bool ContainsTexture(string name) => textures?.ContainsTexture(name) ?? throw new MissingMemberException("TextureArray must be initialized before accessing it.");

        /// <summary>
        /// Creates the texture array with the specified capacity and resolution.
        /// </summary>
        /// <param name="capacity">The maximum number of texture layers.</param>
        /// <param name="resolution">The resolution of each texture layer.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the texture array has already been created.
        /// </exception>
        public void CreateTextureArray(int capacity, TextureResolution resolution)
        {
            if (textures is not null) throw new InvalidOperationException("TextureArray has already been created.");
            textures = new TextureArray(capacity, resolution);
        }

        /// <summary>
        /// Creates the texture array with the specified resolution and capacity.
        /// </summary>
        /// <param name="resolution">The resolution of each texture layer.</param>
        /// <param name="capacity">The maximum number of texture layers.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the texture array has already been created.
        /// </exception>
        public void CreateTextureArray(TextureResolution resolution, int capacity)
        {
            if (textures is not null) throw new InvalidOperationException("TextureArray has already been created.");
            textures = new TextureArray(capacity, resolution);
        }

        /// <summary>
        /// Attempts to load an image and add it to the texture array.
        /// </summary>
        /// <param name="path">The path to the image file.</param>
        /// <param name="name">The name used to identify the texture.</param>
        /// <param name="forceResolution">An optional resolution to use instead of inferring one from the image dimensions.</param>
        /// <param name="type">The pixel type used when converting the image to the target resolution.</param>
        /// <param name="greyScale">Whether to load the image in greyscale.</param>
        /// <returns><see langword="true"/> if the texture was loaded and added successfully, otherwise, <see langword="false"/>.</returns>
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

        /// <summary>
        /// Loads all PNG images in the specified directory into the texture array.
        /// Each texture is named after its file name without the extension.
        /// </summary>
        /// <param name="directory">The directory containing the images.</param>
        /// <param name="type">The pixel type used when converting images to the target resolution.</param>
        /// <param name="forceResolution">An optional resolution to apply to all loaded images.</param>
        /// <param name="greyScale">Whether to load images in greyscale.</param>
        /// <returns><see langword="true"/> if the directory exists, otherwise, <see langword="false"/>.</returns>
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

        /// <summary>
        /// Adds image data to the texture array under the specified name.
        /// </summary>
        /// <param name="data">The image data to upload.</param>
        /// <param name="name">The name used to identify the texture.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the texture array has not been initialized or its capacity has been reached.
        /// </exception>
        public void UploadTexture(ImageData data, string name)
        {
            if (textures is null)
                throw new InvalidOperationException("TextureArray must be created before uploading textures.");

            if (textures.LastLayer >= textures.Capacity)
                throw new InvalidOperationException("TextureArray capacity exceeded.");

            textures.AddTexture(data, name);
        }

        /// <summary>
        /// Attempts to retrieve the texture array layer associated with the specified name.
        /// </summary>
        /// <param name="name">The texture name to search for.</param>
        /// <param name="layer">Receives the texture layer index, or -1 if the texture was not found.</param>
        /// <returns><see langword="true"/> if the texture was found; otherwise, <see langword="false"/>.</returns>
        public bool TryGetTexture(string name, out int layer)
        {
            layer = textures?.GetTextureLayer(name) ?? -1;
            return layer >= 0;
        }

        /// <summary>
        /// Binds the texture array to the specified OpenGL texture unit.
        /// </summary>
        /// <param name="textureUnit">The texture unit to bind to.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the texture unit is outside the supported range.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        /// Thrown if the texture array has not been initialized.
        /// </exception>
        public void Bind(int textureUnit)
        {
            if (_maxTextureUnits < 0) _maxTextureUnits = GL.GetInteger(GetPName.MaxTextureImageUnits);
            if (textureUnit < 0 || textureUnit >= _maxTextureUnits)
                throw new ArgumentOutOfRangeException(nameof(textureUnit));

            if (textures is null)
                throw new KeyNotFoundException($"Texture array not initialized.");

            textures.Bind(textureUnit);
        }

        /// <summary>
        /// Disposes of the texture array and releases its resources.
        /// </summary>
        /// <returns><see langword="true"/> if a texture array was disposed, otherwise <see langword="false"/>.</returns>
        public bool TryDeleteTextures()
        {
            if (textures is null) return false;
            textures.Dispose();
            textures = null;
            return true;
        }
    }
}