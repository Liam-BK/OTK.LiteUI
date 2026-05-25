using System.Xml.Linq;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OTK.LiteUI.UI.Utilities;

namespace OTK.LiteUI.UI.Rendering
{
    public class Material
    {
        public int Shader { private get; set; }

        private Dictionary<string, float> _floatUniforms = new Dictionary<string, float>();
        private Dictionary<string, int> _intUniforms = new Dictionary<string, int>();
        private Dictionary<string, Vector2> _vec2Uniforms = new Dictionary<string, Vector2>();
        private Dictionary<string, Vector3> _vec3Uniforms = new Dictionary<string, Vector3>();
        private Dictionary<string, Vector4> _vec4Uniforms = new Dictionary<string, Vector4>();
        private Dictionary<string, Matrix4> _matrix4Uniforms = new Dictionary<string, Matrix4>();

        private Dictionary<TextureResolution, int> _textureArrays = new();

        public bool DepthTest = true;
        public bool Blending = false;
        public bool CullFace = true;

        private readonly int MaxUnits = GL.GetInteger(GetPName.MaxTextureImageUnits);

        public static Material Load(string path)
        {
            XDocument doc = XDocument.Load(ResourceIO.GetLoadingStream(path) ?? throw new FileNotFoundException($"Path {path} not found. Make sure the name of the file is correct prior to loading."));
            XElement root = doc.Element("Material") ?? throw new Exception("Invalid material file. missing <Material> root.");
            Material material = new Material();

            XElement shaders = root.Element("Shaders") ?? throw new Exception("Material missing <Shaders>");

            string vertexPath = shaders.Element("Vertex")?.Attribute("Path")?.Value ?? throw new Exception("Material missing vertex shader path");
            string fragmentPath = shaders.Element("Fragment")?.Attribute("Path")?.Value ?? throw new Exception("Material missing fragment shader path");

            using var vStream = ResourceIO.GetLoadingStream(vertexPath);
            using var fStream = ResourceIO.GetLoadingStream(fragmentPath);

            if (vStream is null) throw new Exception($"Vertex stream could not be created.");
            if (fStream is null) throw new Exception($"Fragment stream could not be created.");

            material.CompileShaderProgram(new StreamReader(vStream).ReadToEnd(), new StreamReader(fStream).ReadToEnd());

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

                    TextureManager.TryLoadTexture(pathAttr, name, out var result, null, EmptyPixelType.Transparent, grayscale);
                }
            }
            return material;
        }

        public void SetFloat(string name, float value)
        {
            _floatUniforms[name] = value;
        }

        public void SetInteger(string name, int value)
        {
            _intUniforms[name] = value;
        }

        public void SetVector2(string name, Vector2 value)
        {
            _vec2Uniforms[name] = value;
        }

        public void SetVector3(string name, Vector3 value)
        {
            _vec3Uniforms[name] = value;
        }

        public void SetVector4(string name, Vector4 value)
        {
            _vec4Uniforms[name] = value;
        }

        public void SetMatrix4(string name, Matrix4 value)
        {
            _matrix4Uniforms[name] = value;
        }

        public void CompileShaderProgram(string vertexShaderSource, string fragmentShaderSource)
        {
            int vertexShader = GL.CreateShader(ShaderType.VertexShader);
            int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(vertexShader, vertexShaderSource);
            GL.ShaderSource(fragmentShader, fragmentShaderSource);
            GL.CompileShader(vertexShader);
            GL.CompileShader(fragmentShader);

            GL.GetShader(vertexShader, ShaderParameter.CompileStatus, out int vertexStatus);
            if (vertexStatus == 0)
            {
                string infoLog = GL.GetShaderInfoLog(vertexShader);
                GL.DeleteShader(vertexShader);
                throw new Exception($"{ShaderType.VertexShader} compilation failed:\n{infoLog}");
            }

            GL.GetShader(fragmentShader, ShaderParameter.CompileStatus, out int fragmentStatus);

            if (fragmentStatus == 0)
            {
                string infoLog = GL.GetShaderInfoLog(fragmentShader);
                GL.DeleteShader(fragmentShader);
                throw new Exception($"{ShaderType.FragmentShader} compilation failed:\n{infoLog}");
            }

            Shader = GL.CreateProgram();

            GL.AttachShader(Shader, vertexShader);
            GL.AttachShader(Shader, fragmentShader);

            GL.LinkProgram(Shader);

            GL.DetachShader(Shader, vertexShader);
            GL.DetachShader(Shader, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        public bool Matrix4Exists(string name)
        {
            return _matrix4Uniforms.ContainsKey(name);
        }

        public void SetTextureArray(TextureResolution res, int unit)
        {
            if (_textureArrays.Count >= MaxUnits && !_textureArrays.ContainsKey(res))
                throw new InvalidOperationException("Too many texture arrays bound.");

            _textureArrays[res] = unit;
        }

        public void UpdateUniforms(bool transpose = false)
        {
            if (Shader == 0) return;
            GL.UseProgram(Shader);
            foreach (var pair in _floatUniforms)
                GL.Uniform1(GL.GetUniformLocation(Shader, pair.Key), pair.Value);
            foreach (var pair in _intUniforms)
                GL.Uniform1(GL.GetUniformLocation(Shader, pair.Key), pair.Value);
            foreach (var pair in _vec2Uniforms)
                GL.Uniform2(GL.GetUniformLocation(Shader, pair.Key), pair.Value);
            foreach (var pair in _vec3Uniforms)
                GL.Uniform3(GL.GetUniformLocation(Shader, pair.Key), pair.Value);
            foreach (var pair in _vec4Uniforms)
                GL.Uniform4(GL.GetUniformLocation(Shader, pair.Key), pair.Value);

            foreach (var pair in _matrix4Uniforms)
            {
                var mat = pair.Value;
                GL.UniformMatrix4(GL.GetUniformLocation(Shader, pair.Key), transpose, ref mat);
            }
        }

        public void Bind()
        {
            GL.UseProgram(Shader);
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

            UpdateUniforms();

            foreach (var pair in _textureArrays)
            {
                TextureResolution res = pair.Key;
                int unit = pair.Value;

                TextureManager.Bind(res, unit);

                // You need a uniform per array (example naming)
                string uniformName = $"uTexture";
                int loc = GL.GetUniformLocation(Shader, uniformName);

                if (loc != -1)
                    GL.Uniform1(loc, unit);
            }
        }
    }
}