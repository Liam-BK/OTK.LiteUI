using OpenTK.Mathematics;

public class HorizontalGridLayout : ILayout
{
    private int _rows;
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

    public HorizontalGridLayout(int rows, Vector2 elementSize, float padding)
    {
        _rows = Math.Max(rows, 1);
        ElementSize = elementSize;
        Padding = padding;
    }

    public void Apply(Vector4 viewport, List<UIComponent> elements)
    {
        float startLeft = viewport.X;
        float startTop = viewport.W;
        float elementHeight = ((viewport.W - viewport.Y) - Padding * Math.Max(_rows - 1, 0)) / _rows;
        float elementWidth = ElementSize.X;

        for (int i = 0; i < elements.Count; i++)
        {
            int column = i / _rows;
            int row = i % _rows;
            float left = startLeft + column * (elementWidth + Padding);
            float right = left + elementWidth;
            float top = startTop - row * (elementHeight + Padding);

            elements[i].Bounds = new Vector4(left, top - elementHeight, right, top);
        }
    }
}