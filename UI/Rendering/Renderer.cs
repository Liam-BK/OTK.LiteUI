using OpenTK.Graphics.OpenGL4;

namespace OTK.LiteUI.UI.Rendering
{
    public enum InstanceAttribType
    {
        Position2D,
        Position3D,
        Size2D,
        Size3D,
        TexCoords,
        Color3,
        Color4,
        Single,
        Vec2,
        Vec3,
        Vec4,
        Mat4
    }

    public class Renderer
    {
        public bool IsVisible = true;
        public Mesh? Mesh = null;
        public Material? Material = null;
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

        public Renderer(Mesh mesh, Material material, InstanceAttribType[] attributeLayout)
        {
            Mesh = mesh;
            Material = material;
            _instanceVBO = GL.GenBuffer();
            attribLayout = attributeLayout;
            SetUpInstanceAttributes();
        }

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

        public void AddInstance<T>(in T data) where T : struct, IInstanceData
        {
            data.Pack(_instanceData);
            if (_instanceData.Count % FloatsPerInstance != 0) throw new ArgumentException("Instance data does not match defined AttribLayout");
            _instanceCount++;
        }

        public void ClearInstances()
        {
            _instanceData.Clear();
        }

        private void UploadInstanceBuffer()
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, _instanceVBO);
            GL.BufferData(BufferTarget.ArrayBuffer, _instanceData.Count * sizeof(float), _instanceData.ToArray(), BufferUsageHint.DynamicDraw);
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

        public void DrawInstances(PrimitiveType primitives = PrimitiveType.Triangles)
        {
            if (!IsVisible || Mesh == null || Material == null)
                return;

            Material.Bind();
            Mesh.Bind();

            UploadInstanceBuffer();

            GL.DrawElementsInstanced(primitives, Mesh.IndexCount, DrawElementsType.UnsignedInt, 0, _instanceCount);

            ClearInstances();
            _instanceCount = 0;
        }

        public void DrawSingle(PrimitiveType primitives = PrimitiveType.Triangles)
        {
            if (!IsVisible || Mesh == null || Material == null)
                return;

            Material.Bind();
            Mesh.Bind();

            GL.DrawElements(primitives, Mesh.IndexCount, DrawElementsType.UnsignedInt, 0);
        }
    }
}