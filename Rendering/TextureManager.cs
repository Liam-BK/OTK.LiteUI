using OpenTK.Graphics.OpenGL4;

public enum TextureResolution
{
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
namespace OTK.LiteUI.Managers
{
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

        public static void CreateResolution(TextureResolution res, int maxLayers)
        {
            int handle = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2DArray, handle);

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

        public static bool TryLoadTexture(string path, string name, EmptyPixelType type = EmptyPixelType.Transparent, bool greyScale = false)
        {
            try
            {
                var data = ImageLoader.LoadImage(path, ImageLoader.Flip.Vertical, greyScale);
                data.ConvertToResolution(InferResolution(data), type);
                UploadTexture(data, name);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool LoadAllTextures(string directory, EmptyPixelType type, bool greyScale = false)
        {
            if (!Directory.Exists(directory))
                return false;

            foreach (var file in Directory.GetFiles(directory))
            {
                if (Path.GetExtension(file).ToLower() != ".png")
                    continue;

                string name = Path.GetFileNameWithoutExtension(file);
                TryLoadTexture(file, name, type, greyScale);
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
}

// private static readonly Dictionary<string, int> _textures = new();
//         private static readonly List<string> _textureNames = new();
//         private static readonly TextureUnit[] units = Enumerable.Range(0, 32).Select(i => TextureUnit.Texture0 + i).ToArray();

//         /// <summary>
//         /// Total number of textures currently in memory.
//         /// </summary>
//         public static int TextureCount
//         {
//             get => _textureNames.Count;
//         }

//         /// <summary>
//         /// Loads a texture from disk, uploads it to the GPU, and stores it in the TextureManager
//         /// under the specified key.
//         /// </summary>
//         /// <param name="texturePath">The file path of the image to load.</param>
//         /// <param name="name">The identifier used to access the texture later.</param>
//         /// <param name="greyscale">If true, the image is converted to greyscale before being uploaded.</param>
//         public static void LoadTexture(string texturePath, string name, bool greyscale = false)
//         {
//             var data = ImageLoader.LoadImage(texturePath, ImageLoader.Flip.Vertical, greyscale);
//             byte[] buffer = data.Pixels;
//             byte maxValue = 0;
//             for (int i = 0; i < buffer.Length; i++)
//             {
//                 if (buffer[i] > maxValue)
//                 {
//                     maxValue = buffer[i];
//                 }
//             }

//             int textureID;
//             GL.GenTextures(1, out textureID);
//             _textures.Add(name, textureID);
//             _textureNames.Add(name);

//             GL.ActiveTexture(TextureUnit.Texture0);
//             GL.BindTexture(TextureTarget.Texture2D, textureID);

//             GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
//             GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

//             GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, buffer);
//         }

//         /// <summary>
//         /// Calls LoadTexture for all pngs within a given directory. Does not handle embedded resources and as such is intended for development purposes.
//         /// </summary>
//         /// <param name="directory">The directory from which to load the images.</param>
//         /// <param name="greyscale">If true, loads all files as greyscale.</param>
//         /// <exception cref="DirectoryNotFoundException">Thrown when the provided directory is not found.</exception>
//         public static void LoadAllTextures(string directory, bool greyscale = false)
//         {
//             if (!Directory.Exists(directory))
//                 throw new DirectoryNotFoundException($"Directory '{directory}' not found.");

//             foreach (var file in Directory.GetFiles(directory))
//             {
//                 string ext = Path.GetExtension(file).ToLower();
//                 if (ext is not ".png") continue;

//                 string name = Path.GetFileNameWithoutExtension(file);
//                 LoadTexture(file, name, greyscale);
//             }
//         }

//         /// <summary>
//         /// Uploads a given <see cref="ImageData"/> to the GPU and stores it in the TextureManager 
//         /// under the specified key.
//         /// </summary>
//         /// <param name="image">The ImageData to upload to the GPU.</param>
//         /// <param name="name">The identifier used to access the texture later.</param>
//         public static void LoadTexture(ImageData image, string name)
//         {
//             byte[] buffer = image.Pixels;
//             byte maxValue = 0;
//             for (int i = 0; i < buffer.Length; i++)
//             {
//                 if (buffer[i] > maxValue)
//                 {
//                     maxValue = buffer[i];
//                 }
//             }

//             int textureID;
//             GL.GenTextures(1, out textureID);
//             _textures.Add(name, textureID);
//             _textureNames.Add(name);

//             GL.ActiveTexture(TextureUnit.Texture0);
//             GL.BindTexture(TextureTarget.Texture2D, textureID);

//             GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
//             GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

//             GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, image.Width, image.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, buffer);
//         }

//         /// <summary>
//         /// Gets the handle of the texture by name.
//         /// </summary>
//         /// <param name="name">The name of the texture.</param>
//         /// <returns>The handle of the texture with the given name.</returns>
//         /// <exception cref="KeyNotFoundException">Thrown when no texture with the given name is found.</exception>
//         public static int GetTexture(string name)
//         {
//             if (!_textures.TryGetValue(name, out int textureID))
//                 throw new KeyNotFoundException($"Texture '{name}' not found.");
//             return textureID;
//         }

//         /// <summary>
//         /// Gets the handle of the texture by index.
//         /// </summary>
//         /// <param name="index">The index of the texture.</param>
//         /// <returns>The handle of the texture at the given index.</returns>
//         /// <exception cref="ArgumentOutOfRangeException">Thrown when the given index exceeds the bounds of _textureNames.</exception>
//         public static int GetTexture(int index)
//         {
//             if (index < 0 || index >= _textureNames.Count)
//             {
//                 throw new ArgumentOutOfRangeException($"Texture index {index} is out of range.");
//             }
//             return _textures[_textureNames[index]];
//         }

//         /// <summary>
//         /// Binds the texture to a given TextureUnit by name. 
//         /// </summary>
//         /// <param name="name">The name of the texture to be bound.</param>
//         /// <param name="textureUnit">The texture unit to bind the texture to.</param>
//         /// <exception cref="KeyNotFoundException">Thrown when no texture with the given name exists.</exception>
//         public static void Bind(string name, int textureUnit)
//         {
//             if (string.IsNullOrEmpty(name))
//             {
//                 return;
//             }
//             if (!_textures.ContainsKey(name))
//             {
//                 throw new KeyNotFoundException($"Texture '{name}' not found.");
//             }

//             GL.ActiveTexture(units[textureUnit]);
//             GL.BindTexture(TextureTarget.Texture2D, GetTexture(name));
//         }

//         /// <summary>
//         /// Unbinds the texture at the given TextureUnit.
//         /// </summary>
//         /// <param name="textureUnit">The TextureUnit to unbind.</param>
//         public static void Unbind(int textureUnit)
//         {
//             GL.ActiveTexture(units[textureUnit]);
//             GL.BindTexture(TextureTarget.Texture2D, 0);
//         }

//         /// <summary>
//         /// Deletes a texture based on the given key.
//         /// </summary>
//         /// <param name="name">The name of the texture to delete.</param>
//         public static void Delete(string name)
//         {
//             GL.DeleteTexture(_textures[name]);
//             _textures.Remove(name);
//             _textureNames.Remove(name);
//         }

//         /// <summary>
//         /// Deletes a single texture from the GPU at the given index.
//         /// </summary>
//         /// <param name="index">The index of the texture to be deleted.</param>
//         public static void Delete(int index)
//         {
//             Delete(_textureNames[index]);
//         }

//         /// <summary>
//         /// Deletes all loaded textures from the GPU.
//         /// </summary>
//         public static void DeleteAll()
//         {
//             for (int i = _textureNames.Count - 1; i >= 0; i--)
//             {
//                 Delete(i);
//             }
//         }
//     }