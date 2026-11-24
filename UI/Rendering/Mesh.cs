using Assimp;
using OpenTK.Graphics.OpenGL4;
using OTK.LiteUI.UI.Utilities;

namespace OTK.LiteUI.UI.Rendering
{
    /// <summary>
    /// Represents a GPU mesh containing vertex and index buffers, with a
    /// vertex array object describing its vertex attribute layout.
    /// Supports loading mesh data from 3D model files.
    /// </summary>
    public class Mesh : IDisposable
    {
        /// <summary>
        /// Specifies the type and component count of a vertex attribute.
        /// </summary>
        public enum VertexAttribType
        {
            /// <summary>A two-component vertex position.</summary>
            Position2,
            /// <summary>A three-component vertex position.</summary>
            Position3,
            /// <summary>A two-component texture coordinate.</summary>
            TexCoords2,
            /// <summary>A three-component vertex normal.</summary>
            Normal3,
            /// <summary>A tangent attribute with three stored components.</summary>
            Tangent4,
            /// <summary>Four bone indices.</summary>
            WeightIndices4,
            /// <summary>Four bone weights.</summary>
            Weight4,
            /// <summary>A three-component vertex colour.</summary>
            Color3,
            /// <summary>A four-component vertex colour.</summary>
            Color4,
            /// <summary>A single floating-point value.</summary>
            Single,
            /// <summary>A two-component vector.</summary>
            Vec2,
            /// <summary>A three-component vector.</summary>
            Vec3,
            /// <summary>A four-component vector.</summary>
            Vec4,
            /// <summary>A four-by-four matrix.</summary>
            Mat4
        }

        /// <summary>The OpenGL vertex array object handle.</summary>
        public int VAO { get; private set; }

        /// <summary>The OpenGL vertex buffer object handle.</summary>
        public int VBO { get; private set; }

        /// <summary>The OpenGL element buffer object handle.</summary>
        public int EBO { get; private set; }

        /// <summary>The number of vertices in the mesh.</summary>
        public int VertexCount { get; private set; }

        /// <summary>The number of indices in the mesh.</summary>
        public int IndexCount { get; private set; }

        /// <summary>The number of triangular faces in the mesh.</summary>
        public int FaceCount { get; private set; }

        /// <summary>The number of vertex attributes in the layout.</summary>
        public int VertexAttributeCount { get; private set; }

        /// <summary>
        /// Creates a mesh from vertex and index data using the specified vertex layout.
        /// Uploads the data to GPU buffers and configures the vertex array object.
        /// </summary>
        /// <param name="vertexData">The interleaved vertex attribute data.</param>
        /// <param name="indices">The indices defining the mesh's triangular faces.</param>
        /// <param name="vertexLayout">The ordered vertex attribute layout.</param>
        /// <param name="hint">The expected usage pattern of the GPU buffers.</param>
        public Mesh(float[] vertexData, int[] indices, VertexAttribType[] vertexLayout, BufferUsageHint hint = BufferUsageHint.StaticDraw)
        {
            VertexAttributeCount = vertexLayout.Length;
            int attributesPerVertex = 0;
            foreach (var value in vertexLayout)
            {
                attributesPerVertex += GetVertexAttribSize(value);
            }
            VertexCount = vertexData.Length / attributesPerVertex;
            IndexCount = indices.Length;
            FaceCount = indices.Length / 3;

            VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);

            VBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexData.Length * sizeof(float), vertexData, hint);

            EBO = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(int), indices, hint);

            int stride = attributesPerVertex * sizeof(float);
            int offset = 0;

            for (int i = 0; i < vertexLayout.Length; i++)
            {
                int size = GetVertexAttribSize(vertexLayout[i]);
                GL.EnableVertexAttribArray(i);
                GL.VertexAttribPointer(i, size, VertexAttribPointerType.Float, false, stride, offset);
                offset += size * sizeof(float);
            }

            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Loads a mesh from a 3D model file and uploads its geometry to the GPU.
        /// Imports the selected mesh with triangulation and vertex processing enabled.
        /// </summary>
        /// <param name="path">The path to the model file.</param>
        /// <param name="sceneMeshIndex">The index of the mesh within the imported scene.</param>
        /// <returns>The loaded mesh.</returns>
        /// <exception cref="FileNotFoundException">
        /// Thrown if the model file cannot be found.
        /// </exception>
        public static Mesh Load(string path, int sceneMeshIndex = 0)
        {
            var stream = ResourceIO.GetLoadingStream(path);

            if (stream is null) throw new FileNotFoundException($"File: {path} could not be found.");

            AssimpContext context = new AssimpContext();
            PostProcessSteps steps = PostProcessSteps.Triangulate | PostProcessSteps.JoinIdenticalVertices | PostProcessSteps.PreTransformVertices | PostProcessSteps.SplitLargeMeshes | PostProcessSteps.ValidateDataStructure;
            Scene scene = context.ImportFileFromStream(stream, steps);
            Assimp.Mesh tempMesh = scene.Meshes[sceneMeshIndex];

            var vertexLayout = new List<VertexAttribType>();
            bool hasNormals = tempMesh.HasNormals;
            bool hasUVs = tempMesh.HasTextureCoords(0);
            bool hasTangents = tempMesh.Tangents.Count > 0;
            bool hasVertexColours = tempMesh.HasVertexColors(0);
            bool hasBones = tempMesh.HasBones;

            vertexLayout.Add(VertexAttribType.Position3);
            if (hasNormals) vertexLayout.Add(VertexAttribType.Normal3);
            if (hasUVs) vertexLayout.Add(VertexAttribType.TexCoords2);
            if (hasTangents) vertexLayout.Add(VertexAttribType.Tangent4);
            if (hasVertexColours) vertexLayout.Add(VertexAttribType.Color4);
            if (hasBones)
            {
                vertexLayout.Add(VertexAttribType.WeightIndices4);//weight indices
                vertexLayout.Add(VertexAttribType.Weight4);//weights
            }

            var vertices = new List<float>();
            var vertexCount = tempMesh.VertexCount;

            for (int i = 0; i < vertexCount; i++)
            {
                var pos = tempMesh.Vertices[i];
                vertices.Add(pos.X);
                vertices.Add(pos.Y);
                vertices.Add(pos.Z);

                if (hasNormals)
                {
                    var n = tempMesh.Normals[i];
                    vertices.Add(n.X);
                    vertices.Add(n.Y);
                    vertices.Add(n.Z);
                }

                if (hasUVs)
                {
                    var uv = tempMesh.TextureCoordinateChannels[0][i];
                    vertices.Add(uv.X);
                    vertices.Add(uv.Y);
                }

                if (hasTangents)
                {
                    var t = tempMesh.Tangents[i];
                    var n = tempMesh.Normals[i];
                    var b = tempMesh.BiTangents[i];

                    float w = Vector3D.Dot(Vector3D.Cross(n, t), b) < 0 ? -1f : 1f;

                    vertices.Add(t.X);
                    vertices.Add(t.Y);
                    vertices.Add(t.Z);
                    vertices.Add(w);
                }

                if (hasVertexColours)
                {
                    var c = tempMesh.VertexColorChannels[0][i];
                    vertices.Add(c.R);
                    vertices.Add(c.G);
                    vertices.Add(c.B);
                    vertices.Add(c.A);
                }
            }

            var indices = new List<int>();

            foreach (var face in tempMesh.Faces)
            {
                indices.Add(face.Indices[0]);
                indices.Add(face.Indices[1]);
                indices.Add(face.Indices[2]);
            }

            return new Mesh(vertices.ToArray(), indices.ToArray(), vertexLayout.ToArray());
        }

        /// <summary>
        /// Gets the number of floating-point components used by the specified vertex attribute.
        /// </summary>
        /// <param name="type">The vertex attribute type.</param>
        /// <returns>The number of components occupied by the attribute.</returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown if the attribute type is unsupported.
        /// </exception>
        public static int GetVertexAttribSize(VertexAttribType type) => type switch
        {
            VertexAttribType.Position2 => 2,
            VertexAttribType.Position3 => 3,
            VertexAttribType.TexCoords2 => 2,
            VertexAttribType.Normal3 => 3,
            VertexAttribType.Tangent4 => 3,
            VertexAttribType.WeightIndices4 => 4,
            VertexAttribType.Weight4 => 4,
            VertexAttribType.Color3 => 3,
            VertexAttribType.Color4 => 4,
            VertexAttribType.Single => 1,
            VertexAttribType.Vec2 => 2,
            VertexAttribType.Vec3 => 3,
            VertexAttribType.Vec4 => 4,
            VertexAttribType.Mat4 => 16,
            _ => throw new ArgumentOutOfRangeException()
        };

        /// <summary>
        /// Binds this mesh's vertex array object.
        /// </summary>
        public void Bind() => GL.BindVertexArray(VAO);

        /// <summary>
        /// Unbinds the currently bound vertex array object.
        /// </summary>
        public void UnBind() => GL.BindVertexArray(0);

        /// <summary>
        /// Disposes of the mesh's OpenGL buffers and vertex array object.
        /// </summary>
        public void Dispose()
        {
            GL.DeleteBuffer(VBO);
            GL.DeleteBuffer(EBO);
            GL.DeleteVertexArray(VAO);
        }
    }
}