using OpenTK.Mathematics;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;

namespace OTK.LiteUI.Layouts
{
    /// <summary>
    /// Arranges elements horizontally using explicitly specified widths for each element.
    /// </summary>
    public class HorizontalSplitLayoutAbsolute : ILayout
    {
        [UIProperty]
        public float Padding { get; set; }

        [UIProperty]
        public float[] Splits { get; }

        /// <summary>
        /// Creates a horizontal split layout using the specified element widths.
        /// </summary>
        /// <param name="padding">The spacing between elements.</param>
        /// <param name="splits">The widths assigned to elements, cycled through as necessary.</param>
        /// <exception cref="ArgumentException">Thrown when no split values are provided.</exception>
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