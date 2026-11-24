using System.Xml.Linq;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OTK.LiteUI.UI.Rendering.Backend;
using OTK.LiteUI.UI.Utilities;

namespace OTK.LiteUI.UI.Rendering
{
    /// <summary>
    /// Represents a rendering material containing a shader program, configurable
    /// uniform values, and OpenGL render state settings.
    /// </summary>
    public class Material
    {
        private Shader? shader = null;

        private Dictionary<string, float> _floatUniforms = new Dictionary<string, float>();
        private Dictionary<string, int> _intUniforms = new Dictionary<string, int>();
        private Dictionary<string, Vector2> _vec2Uniforms = new Dictionary<string, Vector2>();
        private Dictionary<string, Vector3> _vec3Uniforms = new Dictionary<string, Vector3>();
        private Dictionary<string, Vector4> _vec4Uniforms = new Dictionary<string, Vector4>();
        private Dictionary<string, Matrix4> _matrix4Uniforms = new Dictionary<string, Matrix4>();

        /// <summary>
        /// Whether depth testing is enabled when the material is bound.
        /// </summary>
        public bool DepthTest = true;

        /// <summary>
        /// Whether alpha blending is enabled when the material is bound.
        /// </summary>
        public bool Blending = false;

        /// <summary>
        /// Whether face culling is enabled when the material is bound.
        /// </summary>
        public bool CullFace = true;

        private readonly int MaxUnits = GL.GetInteger(GetPName.MaxTextureImageUnits);

        /// <summary>
        /// Loads a material definition from an XML file, including its shaders,
        /// render state settings, and referenced textures.
        /// </summary>
        /// <param name="path">The path to the material definition file.</param>
        /// <param name="manager">The texture manager used to load referenced textures.</param>
        /// <returns>The loaded material.</returns>
        /// <exception cref="FileNotFoundException">
        /// Thrown if the material definition file cannot be found.
        /// </exception>
        public static Material Load(string path, TextureManager manager)
        {
            XDocument doc = XDocument.Load(ResourceIO.GetLoadingStream(path) ?? throw new FileNotFoundException($"Path {path} not found. Make sure the name of the file is correct prior to loading."));
            XElement root = doc.Element("Material") ?? throw new Exception("Invalid material file. missing <Material> root.");
            Material material = new Material();

            XElement shaders = root.Element("Shaders") ?? throw new Exception("Material missing <Shaders>");

            string vertexPath = shaders.Element("Vertex")?.Attribute("Path")?.Value ?? throw new Exception("Material missing vertex shader path");
            string fragmentPath = shaders.Element("Fragment")?.Attribute("Path")?.Value ?? throw new Exception("Material missing fragment shader path");

            using var vStream = ResourceIO.GetLoadingStream(vertexPath);
            using var fStream = ResourceIO.GetLoadingStream(fragmentPath);

            if (vStream is null) throw new Exception($"Vertex stream at {vertexPath} could not be created.");
            if (fStream is null) throw new Exception($"Fragment stream at {fragmentPath} could not be created.");

            material.shader = new Shader(new StreamReader(vStream).ReadToEnd(), new StreamReader(fStream).ReadToEnd());

            XElement? renderState = root.Element("RenderState");

            if (renderState is not null)
            {
                material.Blending = bool.Parse(renderState.Element("Blending")?.Attribute("Value")?.Value ?? material.Blending.ToString());
                material.DepthTest = bool.Parse(renderState.Element("DepthTest")?.Attribute("Value")?.Value ?? material.DepthTest.ToString());
                material.CullFace = bool.Parse(renderState.Element("CullFace")?.Attribute("Value")?.Value ?? material.CullFace.ToString());
            }

            XElement? textures = root.Element("Textures");
            if (textures is not null)
            {
                foreach (var tex in textures.Elements("Texture"))
                {
                    string name = tex.Attribute("name")?.Value ?? throw new Exception("Texture missing name attribute.");
                    string pathAttr = tex.Attribute("path")?.Value ?? throw new Exception($"Texture {name} missing path attribute.");
                    int unit = int.Parse(tex.Attribute("unit")?.Value ?? throw new Exception($"Texture {name} missing unit attribute."));
                    bool grayscale = bool.Parse(tex.Attribute("grayscale")?.Value ?? "false");
                    bool flipX = bool.Parse(tex.Attribute("flipX")?.Value ?? "false");
                    bool flipY = bool.Parse(tex.Attribute("flipY")?.Value ?? "true");

                    int flags = 0;
                    if (flipY) flags |= 1 << 1;
                    if (flipX) flags |= 1 << 2;

                    manager.TryLoadTexture(pathAttr, name, null, EmptyPixelType.Transparent, grayscale);
                }
            }
            return material;
        }

        /// <summary>
        /// Sets a floating-point uniform value.
        /// </summary>
        /// <param name="name">The uniform name.</param>
        /// <param name="value">The value to assign.</param>
        public void SetFloat(string name, float value)
        {
            _floatUniforms[name] = value;
        }

        /// <summary>
        /// Sets an integer uniform value.
        /// </summary>
        /// <param name="name">The uniform name.</param>
        /// <param name="value">The value to assign.</param>
        public void SetInteger(string name, int value)
        {
            _intUniforms[name] = value;
        }

        /// <summary>
        /// Sets a Vector2 uniform value.
        /// </summary>
        /// <param name="name">The uniform name.</param>
        /// <param name="value">The value to assign.</param>
        public void SetVector2(string name, Vector2 value)
        {
            _vec2Uniforms[name] = value;
        }

        /// <summary>
        /// Sets a Vector3 uniform value.
        /// </summary>
        /// <param name="name">The uniform name.</param>
        /// <param name="value">The value to assign.</param>
        public void SetVector3(string name, Vector3 value)
        {
            _vec3Uniforms[name] = value;
        }

        /// <summary>
        /// Sets a Vector4 uniform value.
        /// </summary>
        /// <param name="name">The uniform name.</param>
        /// <param name="value">The value to assign.</param>
        public void SetVector4(string name, Vector4 value)
        {
            _vec4Uniforms[name] = value;
        }

        /// <summary>
        /// Sets a Matrix4 uniform value.
        /// </summary>
        /// <param name="name">The uniform name.</param>
        /// <param name="value">The value to assign.</param>
        public void SetMatrix4(string name, Matrix4 value)
        {
            _matrix4Uniforms[name] = value;
        }

        /// <summary>
        /// Determines whether a Matrix4 uniform with the specified name has been assigned.
        /// </summary>
        /// <param name="name">The uniform name to check.</param>
        /// <returns><see langword="true"/> if a value has been assigned; otherwise, <see langword="false"/>.</returns>
        public bool Matrix4Exists(string name)
        {
            return _matrix4Uniforms.ContainsKey(name);
        }

        /// <summary>
        /// Uploads the stored uniform values to the material's shader program.
        /// </summary>
        /// <param name="transpose">Whether matrix values should be transposed when uploaded.</param>
        /// <exception cref="MissingMemberException">
        /// Thrown if the material has no initialized shader.
        /// </exception>
        public void UpdateUniforms(bool transpose = false)
        {
            if (shader is null) throw new MissingMemberException("Shader has not been initialized");
            shader.Bind();
            foreach (var pair in _floatUniforms)
                GL.Uniform1(GL.GetUniformLocation(shader.Handle, pair.Key), pair.Value);
            foreach (var pair in _intUniforms)
                GL.Uniform1(GL.GetUniformLocation(shader.Handle, pair.Key), pair.Value);
            foreach (var pair in _vec2Uniforms)
                GL.Uniform2(GL.GetUniformLocation(shader.Handle, pair.Key), pair.Value);
            foreach (var pair in _vec3Uniforms)
                GL.Uniform3(GL.GetUniformLocation(shader.Handle, pair.Key), pair.Value);
            foreach (var pair in _vec4Uniforms)
                GL.Uniform4(GL.GetUniformLocation(shader.Handle, pair.Key), pair.Value);

            foreach (var pair in _matrix4Uniforms)
            {
                var mat = pair.Value;
                GL.UniformMatrix4(GL.GetUniformLocation(shader.Handle, pair.Key), transpose, ref mat);
            }
        }

        /// <summary>
        /// Binds the material's shader, applies its render state settings,
        /// and binds the texture array to texture unit zero.
        /// </summary>
        /// <param name="textureManager">The texture manager providing the texture array.</param>
        /// <exception cref="MissingMemberException">
        /// Thrown if the material has no initialized shader.
        /// </exception>
        public void Bind(TextureManager textureManager)
        {
            if (shader is null) throw new MissingMemberException("Shader has not been initialized so it cannot be bound.");
            shader.Bind();
            if (DepthTest) GL.Enable(EnableCap.DepthTest);
            else GL.Disable(EnableCap.DepthTest);
            if (Blending)
            {
                GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
                GL.Enable(EnableCap.Blend);
            }
            else GL.Disable(EnableCap.Blend);
            if (CullFace) GL.Enable(EnableCap.CullFace);
            else GL.Disable(EnableCap.CullFace);

            textureManager.Bind(0);
            string uniformName = $"uTexture";
            int loc = GL.GetUniformLocation(shader.Handle, uniformName);

            if (loc != -1)
                GL.Uniform1(loc, 0);
        }
    }
}