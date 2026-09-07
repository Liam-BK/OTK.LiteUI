using OpenTK.Mathematics;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;

namespace OTK.LiteUI.Layouts
{
    public class HorizontalSplitLayoutProportional : ILayout
    {
        public float Padding { get; set; }

        private float[] Splits { get; }

        public HorizontalSplitLayoutProportional(float padding, params float[] splits)
        {
            Padding = padding;
            Splits = splits;
            if (splits.Length == 0) throw new ArgumentException("HorizontalSplitLayoutProportional splits parameter must have at least one value");
        }

        public void Apply(Vector4 viewport, List<UIComponent> elements)
        {
            float left = viewport.X;
            float top = viewport.W;

            float viewportWidth = viewport.Z - viewport.X;
            float viewportHeight = viewport.W - viewport.Y;
            float totalPadding = MathF.Max(0, elements.Count - 1) * Padding;
            float availableWidth = viewportWidth - totalPadding;

            for (int i = 0; i < elements.Count; i++)
            {
                float split = Splits[i % Splits.Length];
                float childWidth = availableWidth * split;

                elements[i].Bounds = new Vector4(
                    left,
                    top - viewportHeight,
                    left + childWidth,
                    top);

                left += childWidth + Padding;
            }
        }
    }
}