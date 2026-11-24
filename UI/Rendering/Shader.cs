using System.Reflection;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace OTK.LiteUI.UI.Rendering
{
    /// <summary>
    /// Represents a compiled and linked OpenGL shader program, providing methods
    /// for binding the program and setting its uniform values.
    /// </summary>
    internal class Shader : IDisposable
    {
        /// <summary>
        /// Gets the OpenGL handle of the shader program.
        /// </summary>
        public int Handle { get; }

        private readonly Dictionary<string, int> _uniformLocations = new Dictionary<string, int>();

        /// <summary>
        /// Compiles the supplied vertex and fragment shader sources and links them into an OpenGL program.
        /// </summary>
        /// <param name="vertexSource">The GLSL vertex shader source code.</param>
        /// <param name="fragmentSource">The GLSL fragment shader source code.</param>
        /// <exception cref="Exception">A shader fails to compile or the program fails to link.</exception>
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
                var log = GL.GetProgramInfoLog(Handle);
                if (Handle != 0) GL.DeleteProgram(Handle);
                if (vertexShader != 0) GL.DeleteShader(vertexShader);
                if (fragmentShader != 0) GL.DeleteShader(fragmentShader);
                throw new Exception(log);
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
                GL.DeleteShader(shader);
                throw new Exception($"{type} compilation failed:\n{log}");
            }

            return shader;
        }

        /// <summary>
        /// Creates a shader program from vertex and fragment shader streams.
        /// </summary>
        /// <param name="vertexStream">The vertex shader source stream.</param>
        /// <param name="fragmentStream">The fragment shader source stream.</param>
        /// <param name="disposeStreams">Whether to dispose the supplied streams after reading them.</param>
        /// <returns>The compiled and linked shader program.</returns>
        /// <exception cref="Exception">A shader fails to compile or the program fails to link.</exception>
        public static Shader FromStreams(Stream vertexStream, Stream fragmentStream, bool disposeStreams = true)
        {
            using var readerV = new StreamReader(vertexStream, leaveOpen: !disposeStreams);
            using var readerF = new StreamReader(fragmentStream, leaveOpen: !disposeStreams);
            string v = readerV.ReadToEnd();
            string f = readerF.ReadToEnd();
            return new Shader(v, f);
        }

        /// <summary>
        /// Creates a shader program from vertex and fragment shader files.
        /// </summary>
        /// <param name="vertexPath">The path to the vertex shader file.</param>
        /// <param name="fragmentPath">The path to the fragment shader file.</param>
        /// <returns>The compiled and linked shader program.</returns>
        /// <exception cref="IOException">A shader file cannot be read.</exception>
        /// <exception cref="Exception">A shader fails to compile or the program fails to link.</exception>
        public static Shader FromPath(string vertexPath, string fragmentPath)
        {
            var vertexSource = File.ReadAllText(vertexPath);
            var fragmentSource = File.ReadAllText(fragmentPath);
            return new Shader(vertexSource, fragmentSource);
        }

        /// <summary>
        /// Creates a shader program from embedded vertex and fragment shader resources
        /// in the calling assembly.
        /// </summary>
        /// <param name="vertexResource">The manifest resource name of the vertex shader.</param>
        /// <param name="fragmentResource">The manifest resource name of the fragment shader.</param>
        /// <returns>The compiled and linked shader program.</returns>
        /// <exception cref="Exception">A resource cannot be found, or a shader fails to compile or link.</exception>
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

        /// <summary>
        /// Sets an integer uniform value.
        /// </summary>
        /// <param name="uniformName">The name of the uniform.</param>
        /// <param name="value">The value to assign.</param>
        public void SetInt(string uniformName, int value)
        {
            GL.Uniform1(GetUniformLocation(uniformName), value);
        }

        /// <summary>
        /// Sets a floating-point uniform value.
        /// </summary>
        /// <param name="uniformName">The name of the uniform.</param>
        /// <param name="value">The value to assign.</param>
        public void SetFloat(string uniformName, float value)
        {
            GL.Uniform1(GetUniformLocation(uniformName), value);
        }

        /// <summary>
        /// Sets a two-component vector uniform.
        /// </summary>
        /// <param name="uniformName">The name of the uniform.</param>
        /// <param name="value">The vector to assign.</param>
        public void SetVec2(string uniformName, Vector2 value)
        {
            GL.Uniform2(GetUniformLocation(uniformName), value);
        }

        /// <summary>
        /// Sets a three-component vector uniform.
        /// </summary>
        /// <param name="uniformName">The name of the uniform.</param>
        /// <param name="value">The vector to assign.</param>
        public void SetVec3(string uniformName, Vector3 value)
        {
            GL.Uniform3(GetUniformLocation(uniformName), value);
        }

        /// <summary>
        /// Sets a four-component vector uniform.
        /// </summary>
        /// <param name="uniformName">The name of the uniform.</param>
        /// <param name="value">The vector to assign.</param>
        public void SetVec4(string uniformName, Vector4 value)
        {
            GL.Uniform4(GetUniformLocation(uniformName), value);
        }

        /// <summary>
        /// Sets a four-by-four matrix uniform without transposing the matrix.
        /// </summary>
        /// <param name="uniformName">The name of the uniform.</param>
        /// <param name="value">The matrix to assign.</param>
        public void SetMat4(string uniformName, Matrix4 value)
        {
            GL.UniformMatrix4(GetUniformLocation(uniformName), false, ref value);
        }

        /// <summary>
        /// Makes this shader program the active OpenGL program.
        /// </summary>
        public void Bind()
        {
            GL.UseProgram(Handle);
        }

        /// <summary>
        /// Deletes the OpenGL shader program.
        /// </summary>
        public void Dispose()
        {
            GL.DeleteProgram(Handle);
        }
    }
}