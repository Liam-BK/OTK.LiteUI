using OpenTK.Mathematics;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;

public class VerticalSplitLayoutSandwich : ILayout
{
    public float Padding { get; set; }

    private float[] Splits { get; }

    private float AbsoluteTop { get; }

    private float AbsoluteBottom { get; }

    public VerticalSplitLayoutSandwich(float padding, float absoluteHeightTop, float absoluteHeightBottom, params float[] splits)
    {
        Padding = padding;
        Splits = splits;
        AbsoluteTop = absoluteHeightTop;
        AbsoluteBottom = absoluteHeightBottom;
        if (splits.Length == 0) throw new ArgumentException("VerticalSplitLayoutSandwich splits parameter must have at least one value");
    }

    public void Apply(Vector4 viewport, List<UIComponent> elements)
    {
        if (elements.Count == 0)
            return;

        float totalPadding = MathF.Max(0, elements.Count - 1) * Padding;

        float availableHeight =
            viewport.W - viewport.Y
            - AbsoluteTop
            - AbsoluteBottom
            - totalPadding;

        float top = viewport.W;

        // Top
        elements[0].Bounds = new Vector4(
            viewport.X,
            top - AbsoluteTop,
            viewport.Z,
            top);

        top -= AbsoluteTop + Padding;

        // Middle
        for (int i = 1; i < elements.Count - 1; i++)
        {
            if (!elements[i].IsVisible) continue;
            float height = availableHeight * Splits[(i - 1) % Splits.Length];

            elements[i].Bounds = new Vector4(
                viewport.X,
                top - height,
                viewport.Z,
                top);

            top -= height + Padding;
        }

        // Bottom
        elements[^1].Bounds = new Vector4(
            viewport.X,
            top - AbsoluteBottom,
            viewport.Z,
            top);
    }
}