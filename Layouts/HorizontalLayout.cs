using OpenTK.Mathematics;

public class HorizontalLayout : ILayout
{
    public float MaxScrollInsetMultiplierX => 1;

    public float MaxScrollInsetMultiplierY => -1;

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

    public HorizontalLayout(Vector2 elementSize, float padding)
    {
        ElementSize = elementSize;
        Padding = padding;
    }

    public void Apply(Vector4 viewport, List<UIComponent> elements)
    {
        float x = viewport.X + Padding;
        float w = viewport.W - Padding;
        foreach (var element in elements)
        {
            element.Bounds = new Vector4(x, w - ElementSize.Y, x + ElementSize.X, w);
            x += ElementSize.X + Padding;
        }
    }
}