using OpenTK.Mathematics;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;

namespace OTK.LiteUI.Layouts
{
    /// <summary>
    /// Arranges elements horizontally, assigning the first element a fixed width and distributing the remaining width proportionally.
    /// </summary>
    public class HorizontalSplitLayoutAbsoluteFirst : ILayout
    {
        [UIProperty]
        public float Padding { get; set; }

        [UIProperty]
        public float[] Splits { get; set; }

        /// <summary>
        /// The fixed width assigned to the first element.
        /// </summary>
        [UIProperty]
        public float AbsoluteWidth { get; set; }

        /// <summary>
        /// Creates a horizontal split layout with a fixed-width first element and proportional remaining elements.
        /// </summary>
        /// <param name="padding">The spacing between elements.</param>
        /// <param name="absoluteWidth">The fixed width of the first element.</param>
        /// <param name="splits">The proportions used to distribute the remaining width, cycled through as necessary.</param>
        /// <exception cref="ArgumentException">Thrown when no split values are provided.</exception>
        public HorizontalSplitLayoutAbsoluteFirst(float padding, float absoluteWidth, params float[] splits)
        {
            Padding = padding;
            Splits = splits;
            AbsoluteWidth = absoluteWidth;
            if (splits.Length == 0) throw new ArgumentException("HorizontalSplitLayoutAbsoluteFirst splits parameter must have at least one value");
        }

        public void Apply(Vector4 viewport, List<UIComponent> elements)
        {
            float left = viewport.X;
            float top = viewport.W;

            float viewportWidth = viewport.Z - viewport.X;
            float viewportHeight = viewport.W - viewport.Y;
            float totalPadding = MathF.Max(0, elements.Count - 1) * Padding;
            float availableWidth = viewportWidth - AbsoluteWidth - totalPadding;

            for (int i = 0; i < elements.Count; i++)
            {
                if (!elements[i].IsVisible) continue;
                float childWidth;

                if (i == 0)
                {
                    childWidth = AbsoluteWidth;
                }
                else
                {
                    float split = Splits[(i - 1) % Splits.Length];
                    childWidth = availableWidth * split;
                }

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