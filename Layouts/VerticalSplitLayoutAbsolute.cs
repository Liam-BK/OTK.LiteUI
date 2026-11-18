using OpenTK.Mathematics;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;

namespace OTK.LiteUI.Layouts
{
    public class VerticalSplitLayoutAbsolute : ILayout
    {
        [UIProperty]
        public float Padding { get; set; }

        [UIProperty]
        public float[] Splits { get; set; }

        public VerticalSplitLayoutAbsolute(float padding, params float[] splits)
        {
            Padding = padding;
            Splits = splits;
            if (splits.Length == 0) throw new ArgumentException("VerticalSplitLayoutAbsolute splits parameter must have at least one value");
        }


        public void Apply(Vector4 viewport, List<UIComponent> elements)
        {
            int splitIndex = 0;
            float left = viewport.X;
            float top = viewport.W;
            float elementWidth = viewport.Z - viewport.X;
            foreach (var element in elements)
            {
                if (!element.IsVisible) continue;
                float childHeight = Splits[splitIndex % Splits.Length];
                element.Bounds = new Vector4(left, top - childHeight, left + elementWidth, top);
                top -= childHeight + Padding;
                splitIndex++;
            }
        }
    }
}