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

    public Vector4 LayoutBounds
    {
        get;
        set;
    }

    public VerticalLayout(Vector4 layoutBounds, Vector2 elementSize, float padding)
    {
        LayoutBounds = layoutBounds;
        ElementSize = elementSize;
        Padding = padding;
    }

    public void Apply(List<UIComponent> elements)
    {
        float x = LayoutBounds.X + Padding;
        float w = LayoutBounds.W - Padding;
        float boundsWidth = (LayoutBounds.Z - LayoutBounds.X) - 2 * Padding;
        foreach (var element in elements)
        {
            element.Bounds = new Vector4(x, w - ElementSize.Y, x + boundsWidth, w);
            w -= ElementSize.Y + Padding;
        }
    }
}