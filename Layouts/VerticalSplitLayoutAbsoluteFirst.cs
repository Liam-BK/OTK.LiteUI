using OpenTK.Mathematics;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;

namespace OTK.LiteUI.Layouts
{
    public class VerticalSplitLayoutAbsoluteFirst : ILayout
    {
        public float Padding { get; set; }

        private float AbsoluteHeight { get; }

        private float[] Splits { get; }


        public VerticalSplitLayoutAbsoluteFirst(float padding, float absoluteHeight, params float[] splits)
        {
            Padding = padding;
            Splits = splits;
            AbsoluteHeight = absoluteHeight;
            if (splits.Length == 0) throw new ArgumentException("VerticalSplitLayoutAbsoluteFirst splits parameter must have at least one value");
        }

        public void Apply(Vector4 viewport, List<UIComponent> elements)
        {
            float left = viewport.X;
            float top = viewport.W;

            float viewportHeight = viewport.W - viewport.Y;
            float totalPadding = MathF.Max(0, elements.Count - 1) * Padding;
            float availableHeight = viewportHeight - AbsoluteHeight - totalPadding;

            for (int i = 0; i < elements.Count; i++)
            {
                if (!elements[i].IsVisible) continue;
                float childHeight;

                if (i == 0)
                {
                    childHeight = AbsoluteHeight;
                }
                else
                {
                    float split = Splits[(i - 1) % Splits.Length];
                    childHeight = availableHeight * split;
                }

                elements[i].Bounds = new Vector4(
                    left,
                    top - childHeight,
                    viewport.Z,
                    top);

                top -= childHeight + Padding;
            }
        }
    }
}