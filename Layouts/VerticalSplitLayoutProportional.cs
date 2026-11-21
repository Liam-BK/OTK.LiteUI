using OpenTK.Mathematics;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;

namespace OTK.LiteUI.Layouts
{
    /// <summary>
    /// Arranges elements vertically, distributing the available height proportionally between them.
    /// </summary>
    public class VerticalSplitLayoutProportional : ILayout
    {
        [UIProperty]
        public float Padding { get; set; }

        /// <summary>
        /// The proportions used to determine element heights, cycled through as necessary.
        /// </summary>
        [UIProperty]
        public float[] Splits { get; set; }

        /// <summary>
        /// Creates a proportional vertical split layout.
        /// </summary>
        /// <param name="padding">The spacing between elements.</param>
        /// <param name="splits">The proportions used to determine element heights, cycled through as necessary.</param>
        /// <exception cref="ArgumentException">Thrown when no split values are provided.</exception>
        public VerticalSplitLayoutProportional(float padding, params float[] splits)
        {
            Padding = padding;
            Splits = splits;
            if (splits.Length == 0) throw new ArgumentException("VerticalSplitLayoutProportional splits parameter must have at least one value");
        }

        public void Apply(Vector4 viewport, List<UIComponent> elements)
        {
            float left = viewport.X;
            float top = viewport.W;

            float viewportHeight = viewport.W - viewport.Y;
            float totalPadding = MathF.Max(0, elements.Count - 1) * Padding;
            float availableHeight = viewportHeight - totalPadding;

            for (int i = 0; i < elements.Count; i++)
            {
                if (!elements[i].IsVisible) continue;
                float split = Splits[i % Splits.Length];
                float childHeight = availableHeight * split;

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