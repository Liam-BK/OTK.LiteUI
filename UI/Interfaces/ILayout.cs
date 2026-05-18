using OpenTK.Mathematics;

public interface ILayout
{
    public float Padding
    {
        get;
        set;
    }

    public Vector2 ElementSize
    {
        get;
        set;
    }

    public void Apply(Vector4 viewport, List<UIComponent> elements);
}