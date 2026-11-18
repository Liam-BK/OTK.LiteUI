using OpenTK.Mathematics;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;

public class HorizontalSplitLayoutSandwich : ILayout
{
    [UIProperty]
    public float Padding { get; set; }

    [UIProperty]
    public float[] Splits { get; set; }

    [UIProperty]
    public float AbsoluteLeft { get; set; }

    [UIProperty]
    public float AbsoluteRight { get; set; }

    public HorizontalSplitLayoutSandwich(float padding, float absoluteWidthLeft, float absoluteWidthRight, params float[] splits)
    {
        Padding = padding;
        Splits = splits;
        AbsoluteLeft = absoluteWidthLeft;
        AbsoluteRight = absoluteWidthRight;
        if (splits.Length == 0) throw new ArgumentException("HorizontalSplitLayoutSandwich splits parameter must have at least one value");
    }

    public void Apply(Vector4 viewport, List<UIComponent> elements)
    {
        if (elements.Count == 0)
            return;

        float totalPadding = MathF.Max(0, elements.Count - 1) * Padding;

        float availableWidth =
            viewport.Z - viewport.X
            - AbsoluteLeft
            - AbsoluteRight
            - totalPadding;

        float left = viewport.X;

        // Absolute left
        elements[0].Bounds = new Vector4(
            left,
            viewport.Y,
            left + AbsoluteLeft,
            viewport.W);

        left += AbsoluteLeft + Padding;

        // Proportional middle
        for (int i = 1; i < elements.Count - 1; i++)
        {
            if (!elements[i].IsVisible) continue;
            float width = availableWidth * Splits[(i - 1) % Splits.Length];

            elements[i].Bounds = new Vector4(
                left,
                viewport.Y,
                left + width,
                viewport.W);

            left += width + Padding;
        }

        // Absolute right
        elements[^1].Bounds = new Vector4(
            left,
            viewport.Y,
            left + AbsoluteRight,
            viewport.W);
    }
}