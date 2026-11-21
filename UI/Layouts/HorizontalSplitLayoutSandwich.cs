using OpenTK.Mathematics;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;

namespace OTK.LiteUI.Layouts
{
    /// <summary>
    /// Arranges elements horizontally with fixed-width elements at both ends and proportionally sized elements in between.
    /// </summary>
    public class HorizontalSplitLayoutSandwich : ILayout
    {
        [UIProperty]
        public float Padding { get; set; }

        [UIProperty]
        public float[] Splits { get; set; }

        /// <summary>
        /// The fixed width assigned to the first element.
        /// </summary>
        [UIProperty]
        public float AbsoluteLeft { get; set; }

        /// <summary>
        /// The fixed width assigned to the last element.
        /// </summary>
        [UIProperty]
        public float AbsoluteRight { get; set; }

        /// <summary>
        /// Creates a horizontal split layout with fixed-width elements at both ends and proportional elements in between.
        /// </summary>
        /// <param name="padding">The spacing between elements.</param>
        /// <param name="absoluteWidthLeft">The fixed width of the first element.</param>
        /// <param name="absoluteWidthRight">The fixed width of the last element.</param>
        /// <param name="splits">The proportions used to distribute the remaining width, cycled through as necessary.</param>
        /// <exception cref="ArgumentException">Thrown when no split values are provided.</exception>
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
            int skipCount = 0;

            if (elements[0].IsVisible)
            {
                // Absolute left
                elements[0].Bounds = new Vector4(
                    left,
                    viewport.Y,
                    left + AbsoluteLeft,
                    viewport.W);

                left += AbsoluteLeft + Padding;
            }
            else skipCount = 1;
            // Proportional middle
            for (int i = 1; i < elements.Count - 1; i++)
            {
                if (!elements[i].IsVisible)
                {
                    skipCount++;
                    continue;
                }
                float width = availableWidth * Splits[(i - skipCount - 1) % Splits.Length];

                elements[i].Bounds = new Vector4(
                    left,
                    viewport.Y,
                    left + width,
                    viewport.W);

                left += width + Padding;
            }

            if (!elements[^1].IsVisible) return;
            // Absolute right
            elements[^1].Bounds = new Vector4(
                left,
                viewport.Y,
                left + AbsoluteRight,
                viewport.W);
        }
    }
}