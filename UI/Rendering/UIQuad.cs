using OpenTK.Mathematics;

public struct UIQuad : IInstanceData
{
    public Vector2 position;
    public Vector2 size;
    public Vector2 UVOffset;
    public Vector2 UVRange;
    public Vector4 colour;
    public float textureLayer;
    public void Pack(List<float> ToPack)
    {
        Instancing.WriteToBuffer(ToPack, position);
        Instancing.WriteToBuffer(ToPack, size);
        Instancing.WriteToBuffer(ToPack, UVOffset);
        Instancing.WriteToBuffer(ToPack, UVRange);
        Instancing.WriteToBuffer(ToPack, colour);
        Instancing.WriteToBuffer(ToPack, textureLayer);
    }
}