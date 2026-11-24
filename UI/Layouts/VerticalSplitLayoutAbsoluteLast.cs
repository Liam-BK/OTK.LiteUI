using OpenTK.Mathematics;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;

namespace OTK.LiteUI.Layouts
{
    /// <summary>
    /// Arranges elements vertically, distributing the available height proportionally and assigning the last element a fixed height.
    /// </summary>
    public class VerticalSplitLayoutAbsoluteLast : ILayout
    {
        [UIProperty]
        public float Padding { get; set; }

        /// <summary>
        /// The fixed height assigned to the last element.
        /// </summary>
        [UIProperty]
        public float AbsoluteHeight { get; set; }

        /// <summary>
        /// The proportions used to distribute the remaining height, cycled through as necessary.
        /// </summary>
        [UIProperty]
        public float[] Splits { get; set; }

        /// <summary>
        /// Creates a vertical split layout with proportionally sized elements and a fixed-height last element.
        /// </summary>
        /// <param name="padding">The spacing between elements.</param>
        /// <param name="absoluteHeight">The fixed height of the last element.</param>
        /// <param name="splits">The proportions used to distribute the remaining height, cycled through as necessary.</param>
        /// <exception cref="ArgumentException">Thrown when no split values are provided.</exception>
        public VerticalSplitLayoutAbsoluteLast(float padding, float absoluteHeight, params float[] splits)
        {
            Padding = padding;
            AbsoluteHeight = absoluteHeight;
            Splits = splits;
            if (splits.Length == 0) throw new ArgumentException("VerticalSplitLayoutAbsoluteLast splits parameter must have at least one value");
        }

        public void Apply(Vector4 viewport, List<UIComponent> elements)
        {
            float left = viewport.X;
            float top = viewport.W;

            float invisibleElements = 0;
            foreach (var element in elements)
            {
                if (!element.IsVisible) invisibleElements++;
            }
            float viewportHeight = viewport.W - viewport.Y;
            float totalPadding = MathF.Max(0, elements.Count - invisibleElements - 1) * Padding;
            float availableHeight = viewportHeight - AbsoluteHeight - totalPadding;

            for (int i = 0; i < elements.Count; i++)
            {
                if (!elements[i].IsVisible) continue;
                float childHeight;

                if (i == elements.Count - 1)
                {
                    childHeight = AbsoluteHeight;
                }
                else
                {
                    float split = Splits[i % Splits.Length];
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