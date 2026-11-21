using OpenTK.Mathematics;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;

namespace OTK.LiteUI.Layouts
{
    /// <summary>
    /// Arranges elements vertically using explicitly specified heights for each element.
    /// </summary>
    public class VerticalSplitLayoutAbsolute : ILayout
    {
        [UIProperty]
        public float Padding { get; set; }

        /// <summary>
        /// The absolute heights assigned to elements, cycled through as necessary.
        /// </summary>
        [UIProperty]
        public float[] Splits { get; set; }

        /// <summary>
        /// Creates a vertical split layout using the specified element heights.
        /// </summary>
        /// <param name="padding">The spacing between elements.</param>
        /// <param name="splits">The heights assigned to elements, cycled through as necessary.</param>
        /// <exception cref="ArgumentException">Thrown when no split values are provided.</exception>
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