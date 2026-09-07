using System.Reflection;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OTK.LiteUI.UI.Rendering
{
    internal class Shader : IDisposable
    {
        public int Handle { get; }

        private readonly Dictionary<string, int> _uniformLocations = new Dictionary<string, int>();

        public Shader(string vertexSource, string fragmentSource)
        {
            var vertexShader = Compile(ShaderType.VertexShader, vertexSource);
            var fragmentShader = Compile(ShaderType.FragmentShader, fragmentSource);

            Handle = GL.CreateProgram();

            GL.AttachShader(Handle, vertexShader);
            GL.AttachShader(Handle, fragmentShader);

            GL.LinkProgram(Handle);

            GL.GetProgram(Handle, GetProgramParameterName.LinkStatus, out int success);
            if (success == 0)
            {
                throw new Exception(GL.GetProgramInfoLog(Handle));
            }

            GL.DetachShader(Handle, vertexShader);
            GL.DetachShader(Handle, fragmentShader);

            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
        }

        private int Compile(ShaderType type, string source)
        {
            int shader = GL.CreateShader(type);

            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            GL.GetShader(shader, ShaderParameter.CompileStatus, out int success);

            if (success == 0)
            {
                string log = GL.GetShaderInfoLog(shader);
                throw new Exception($"{type} compilation failed:\n{log}");
            }

            return shader;
        }

        public static Shader FromStreams(Stream vertexStream, Stream fragmentStream, bool disposeStreams = true)
        {
            using var readerV = new StreamReader(vertexStream, leaveOpen: !disposeStreams);
            using var readerF = new StreamReader(fragmentStream, leaveOpen: !disposeStreams);
            string v = readerV.ReadToEnd();
            string f = readerF.ReadToEnd();
            return new Shader(v, f);
        }

        public static Shader FromPath(string vertexPath, string fragmentPath)
        {
            var vertexSource = File.ReadAllText(vertexPath);
            var fragmentSource = File.ReadAllText(fragmentPath);
            return new Shader(vertexSource, fragmentSource);
        }

        public static Shader FromEmbeddedResource(string vertexResource, string fragmentResource)
        {
            var asm = Assembly.GetCallingAssembly();

            using var vStream = asm.GetManifestResourceStream(vertexResource);
            using var fStream = asm.GetManifestResourceStream(fragmentResource);

            if (vStream == null || fStream == null)
                throw new Exception("Shader resource not found.");

            return FromStreams(vStream, fStream);
        }

        private int GetUniformLocation(string uniformName)
        {
            if (_uniformLocations.TryGetValue(uniformName, out var location)) return location;
            location = GL.GetUniformLocation(Handle, uniformName);
            if (location != -1) _uniformLocations[uniformName] = location;
            else Console.WriteLine($"uniform: ({uniformName}) does not exist in the current shader.");
            return location;
        }

        public void SetInt(string uniformName, int value)
        {
            GL.Uniform1(GetUniformLocation(uniformName), value);
        }

        public void SetFloat(string uniformName, float value)
        {
            GL.Uniform1(GetUniformLocation(uniformName), value);
        }

        public void SetVec2(string uniformName, Vector2 value)
        {
            GL.Uniform2(GetUniformLocation(uniformName), value);
        }

        public void SetVec3(string uniformName, Vector3 value)
        {
            GL.Uniform3(GetUniformLocation(uniformName), value);
        }

        public void SetVec4(string uniformName, Vector4 value)
        {
            GL.Uniform4(GetUniformLocation(uniformName), value);
        }

        public void SetMat4(string uniformName, Matrix4 value)
        {
            GL.UniformMatrix4(GetUniformLocation(uniformName), false, ref value);
        }

        public void Bind()
        {
            GL.UseProgram(Handle);
        }

        public void Dispose()
        {
            GL.DeleteProgram(Handle);
        }
    }
}