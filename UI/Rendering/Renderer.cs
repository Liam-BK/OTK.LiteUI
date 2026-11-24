using OpenTK.Graphics.OpenGL4;
using OTK.LiteUI.UI.Rendering.Backend;

namespace OTK.LiteUI.UI.Rendering
{
    /// <summary>
    /// Defines the supported data types for per-instance vertex attributes.
    /// </summary>
    public enum InstanceAttribType
    {
        /// <summary>A two-component instance position.</summary>
        Position2D,

        /// <summary>A three-component instance position.</summary>
        Position3D,

        /// <summary>A two-component instance size.</summary>
        Size2D,

        /// <summary>A three-component instance size.</summary>
        Size3D,

        /// <summary>A two-component texture coordinate.</summary>
        TexCoords,

        /// <summary>A three-component colour.</summary>
        Color3,

        /// <summary>A four-component colour.</summary>
        Color4,

        /// <summary>A single floating-point value.</summary>
        Single,

        /// <summary>A two-component vector.</summary>
        Vec2,

        /// <summary>A three-component vector.</summary>
        Vec3,

        /// <summary>A four-component vector.</summary>
        Vec4,

        /// <summary>A four-by-four matrix, occupying four vertex attribute locations.</summary>
        Mat4
    }

    /// <summary>
    /// Manages a mesh and material for rendering instances using instanced drawing.
    /// Instance data is packed into a buffer and supplied as per-instance vertex attributes.
    /// </summary>
    public class Renderer
    {
        /// <summary>
        /// Gets or sets whether this renderer should submit draw calls.
        /// </summary>
        public bool IsVisible = true;

        /// <summary>
        /// Gets or sets the mesh used for rendering.
        /// </summary>
        public Mesh? Mesh = null;

        /// <summary>
        /// Gets or sets the material used for rendering.
        /// </summary>
        public Material? Material = null;

        /// <summary>
        /// Gets the number of complete instances currently stored in the instance data buffer.
        /// </summary>
        public int InstanceCount => _instanceData.Count / attribLayout.Length;
        private List<float> _instanceData = new List<float>();
        private int _instanceVBO;
        private int _instanceCount = 0;
        private InstanceAttribType[] attribLayout;

        private int FloatsPerInstance
        {
            get
            {
                int count = 0;
                foreach (var a in attribLayout)
                    count += GetInstanceAttribSize(a);
                return count;
            }
        }

        private int InstanceStride
        {
            get
            {
                return FloatsPerInstance * sizeof(float);
            }
        }

        /// <summary>
        /// Creates a renderer for the specified mesh and material, configuring its per-instance vertex attributes.
        /// </summary>
        /// <param name="mesh">The mesh to render.</param>
        /// <param name="material">The material to use when rendering.</param>
        /// <param name="attributeLayout">The ordered layout of per-instance vertex attributes.</param>
        public Renderer(Mesh mesh, Material material, InstanceAttribType[] attributeLayout)
        {
            Mesh = mesh;
            Material = material;
            _instanceVBO = GL.GenBuffer();
            attribLayout = attributeLayout;
            SetUpInstanceAttributes();
        }

        /// <summary>
        /// Gets the number of floating-point values occupied by the specified instance attribute type.
        /// </summary>
        /// <param name="type">The instance attribute type.</param>
        /// <returns>The number of floating-point values required by the attribute.</returns>
        /// <exception cref="ArgumentOutOfRangeException">The specified attribute type is not supported.</exception>
        public static int GetInstanceAttribSize(InstanceAttribType type) => type switch
        {
            InstanceAttribType.Position2D => 2,
            InstanceAttribType.Position3D => 3,
            InstanceAttribType.Size2D => 2,
            InstanceAttribType.Size3D => 3,
            InstanceAttribType.TexCoords => 2,
            InstanceAttribType.Color3 => 3,
            InstanceAttribType.Color4 => 4,
            InstanceAttribType.Single => 1,
            InstanceAttribType.Vec2 => 2,
            InstanceAttribType.Vec3 => 3,
            InstanceAttribType.Vec4 => 4,
            InstanceAttribType.Mat4 => 16,
            _ => throw new ArgumentOutOfRangeException()
        };

        /// <summary>
        /// Packs instance data into the renderer's instance buffer.
        /// </summary>
        /// <typeparam name="T">The instance data type.</typeparam>
        /// <param name="data">The instance data to add.</param>
        /// <exception cref="ArgumentException">The packed data length does not match the configured attribute layout.</exception>
        public void AddInstance<T>(in T data) where T : struct, IInstanceData
        {
            data.Pack(_instanceData);
            if (_instanceData.Count % FloatsPerInstance != 0) throw new ArgumentException("Instance data does not match defined AttribLayout");
            _instanceCount++;
        }

        /// <summary>
        /// Removes all pending instance data from the renderer.
        /// </summary>
        public void ClearInstances()
        {
            _instanceData.Clear();
        }

        private void UploadInstanceBuffer()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, _instanceData.Count * sizeof(float), _instanceData.ToArray(), BufferUsageHint.StreamDraw);
        }

        private void SetUpInstanceAttributes()
        {
            if (Mesh is null || attribLayout.Length == 0) return;
            Mesh.Bind();

            GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceVBO);

            int offset = 0;
            int attribIndex = Mesh.VertexAttributeCount;

            foreach (var attrib in attribLayout)
            {
                int size = GetInstanceAttribSize(attrib);
                if (attrib == InstanceAttribType.Mat4)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        GL.EnableVertexAttribArray(attribIndex + i);
                        GL.VertexAttribPointer(attribIndex + i, 4, VertexAttribPointerType.Float, false, InstanceStride, (offset + i * 4) * sizeof(float));
                        GL.VertexAttribDivisor(attribIndex + i, 1);
                    }
                    attribIndex += 4;
                }
                else
                {
                    GL.EnableVertexAttribArray(attribIndex);
                    GL.VertexAttribPointer(attribIndex, size, VertexAttribPointerType.Float, false, InstanceStride, offset * sizeof(float));
                    GL.VertexAttribDivisor(attribIndex, 1);
                    attribIndex++;
                }
                offset += size;
            }
        }

        /// <summary>
        /// Draws all pending instances using an instanced draw call, then clears the instance data.
        /// </summary>
        /// <param name="manager">The texture manager used by the material.</param>
        /// <param name="primitives">The primitive type used to draw the mesh.</param>
        public void DrawInstances(TextureManager manager, PrimitiveType primitives = PrimitiveType.Triangles)
        {
            if (!IsVisible || Mesh == null || Material == null)
                return;

            Material.Bind(manager);
            Mesh.Bind();
            UploadInstanceBuffer();

            GL.DrawElementsInstanced(primitives, Mesh.IndexCount, DrawElementsType.UnsignedInt, 0, _instanceCount);

            ClearInstances();
            _instanceCount = 0;
        }

        /// <summary>
        /// Draws the mesh once without using instance data.
        /// </summary>
        /// <param name="textureManager">The texture manager used by the material.</param>
        /// <param name="primitives">The primitive type used to draw the mesh.</param>
        public void DrawSingle(TextureManager textureManager, PrimitiveType primitives = PrimitiveType.Triangles)
        {
            if (!IsVisible || Mesh == null || Material == null)
                return;

            Material.Bind(textureManager);
            Mesh.Bind();

            GL.DrawElements(primitives, Mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
        }
    }
}