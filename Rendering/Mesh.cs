using Assimp;
using OpenTK.Graphics.OpenGL4;

public class Mesh : IDisposable
{
    public enum VertexAttribType
    {
        Position2,
        Position3,
        TexCoords2,
        Normal3,
        Tangent4,
        WeightIndices4,
        Weight4,
        Color3,
        Color4,
        Single,
        Vec2,
        Vec3,
        Vec4,
        Mat4
    }

    public int VAO { get; private set; }
    public int VBO { get; private set; }
    public int EBO { get; private set; }
    public int VertexCount { get; private set; }
    public int IndexCount { get; private set; }
    public int FaceCount { get; private set; }
    public int VertexAttributeCount { get; private set; }

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

    public void Bind() => GL.BindVertexArray(VAO);
    public void UnBind() => GL.BindVertexArray(0);

    public void Dispose()
    {
        GL.DeleteBuffer(VBO);
        GL.DeleteBuffer(EBO);
        GL.DeleteVertexArray(VAO);
    }
}