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

    public Vector4 LayoutBounds
    {
        get;
        set;
    }

    public void Apply(List<UIComponent> elements);
}