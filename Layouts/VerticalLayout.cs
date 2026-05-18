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
        float x = viewport.X + Padding;
        float w = viewport.W - Padding;
        float boundsWidth = (viewport.Z - viewport.X) - 2 * Padding;
        foreach (var element in elements)
        {
            element.Bounds = new Vector4(x, w - ElementSize.Y, x + boundsWidth, w);
            w -= ElementSize.Y + Padding;
        }
    }
}