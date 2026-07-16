using OpenTK.Graphics.OpenGL4;
using OTK.LiteUI.UI.Utilities;

namespace OTK.LiteUI.UI.Rendering
{
    internal class TextureArray : IDisposable
    {
        public int Handle { get; }
        public int TextureSize { get; }
        public int Capacity { get; }
        public int LastLayer { get; private set; } = 0;
        public TextureResolution Resolution { get; }
        public EmptyPixelType emptyPixelType { private get; set; } = EmptyPixelType.Transparent;
        private Dictionary<string, int> _textureIDs = new Dictionary<string, int>();

        public TextureArray(int capacity, TextureResolution resolution)
        {
            Handle = GL.GenTexture();
            Capacity = capacity;
            Resolution = resolution;
            TextureSize = GetResolutionValue(resolution);

            GL.BindTexture(TextureTarget.Texture2DArray, Handle);

            GL.TexImage3D(TextureTarget.Texture2DArray, 0, PixelInternalFormat.Rgba8, TextureSize, TextureSize, capacity, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);

            GL.BindTexture(TextureTarget.Texture2DArray, 0);
        }

        private static int GetResolutionValue(TextureResolution resolution)
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
                _ => 0
            };
        }

        public bool ContainsTexture(string name)
        {
            return _textureIDs.ContainsKey(name);
        }

        public void AddTexture(ImageData data, string name)
        {
            if (_textureIDs.ContainsKey(name)) throw new ArgumentException($"Texture with name \"{name}\" already exists in TextureArray.");

            if (data.Width != TextureSize || data.Height != TextureSize) data.ConvertToResolution(Resolution);
            data.FlipImageVertically();

            _textureIDs.Add(name, LastLayer);

            GL.BindTexture(TextureTarget.Texture2DArray, Handle);

            GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, LastLayer, TextureSize, TextureSize, 1, PixelFormat.Rgba, PixelType.UnsignedByte, data.Pixels);

            NextLayer();

            GL.BindTexture(TextureTarget.Texture2DArray, 0);
        }

        public void Bind(int unit)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + unit);
            GL.BindTexture(TextureTarget.Texture2DArray, Handle);
        }

        private int NextLayer()
        {
            if (LastLayer >= Capacity) throw new InvalidOperationException("TextureArray is full.");

            return LastLayer++;
        }

        public int GetTextureLayer(string textureName)
        {
            if (_textureIDs.TryGetValue(textureName, out var result))
            {
                return result;
            }
            return -1;
        }

        public void Dispose()
        {
            LastLayer = 0;
            _textureIDs.Clear();
            GL.DeleteTexture(Handle);
            GL.BindTexture(TextureTarget.Texture2DArray, 0);
        }
    }
}