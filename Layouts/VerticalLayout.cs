using OpenTK.Mathematics;

public class VerticalLayout : ILayout
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

    public VerticalLayout(Vector2 elementSize, float padding)
    {
        ElementSize = elementSize;
        Padding = padding;
    }

    public void Apply(Vector4 viewport, List<UIComponent> elements)
    {
        float left = viewport.X;
        float top = viewport.W;
        float elementWidth = (viewport.Z - viewport.X);
        foreach (var element in elements)
        {
            element.Bounds = new Vector4(left, top - ElementSize.Y, left + elementWidth, top);
            top -= ElementSize.Y + Padding;
        }
    }
}