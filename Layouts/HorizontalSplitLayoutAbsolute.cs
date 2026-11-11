using OpenTK.Mathematics;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;

namespace OTK.LiteUI.Layouts
{
    public class HorizontalSplitLayoutAbsolute : ILayout
    {
        public float Padding { get; set; }
        private float[] Splits { get; }

        public HorizontalSplitLayoutAbsolute(float padding, params float[] splits)
        {
            Padding = padding;
            Splits = splits;
            if (splits.Length == 0) throw new ArgumentException("HorizontalSplitLayoutAbsolute splits parameter must have at least one value");
        }

        public void Apply(Vector4 viewport, List<UIComponent> elements)
        {
            int splitIndex = 0;
            float left = viewport.X;
            float top = viewport.W;
            float viewportHeight = top - viewport.Y;
            foreach (var element in elements)
            {
                if (!element.IsVisible) continue;
                var childWidth = Splits[splitIndex % Splits.Length];
                element.Bounds = new Vector4(left, top - viewportHeight, left + childWidth, top);
                left += childWidth + Padding;
                splitIndex++;
            }
        }
    }
}