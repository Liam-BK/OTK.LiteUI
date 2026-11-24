using OpenTK.Mathematics;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;

namespace OTK.LiteUI.Layouts
{
    /// <summary>
    /// Arranges elements horizontally, distributing the available width proportionally between them.
    /// </summary>
    public class HorizontalSplitLayoutProportional : ILayout
    {
        [UIProperty]
        public float Padding { get; set; }

        [UIProperty]
        public float[] Splits { get; set; }

        /// <summary>
        /// Creates a proportional horizontal split layout.
        /// </summary>
        /// <param name="padding">The spacing between elements.</param>
        /// <param name="splits">The proportions used to determine element widths, cycled through as necessary.</param>
        /// <exception cref="ArgumentException">Thrown when no split values are provided.</exception>
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
                if (!elements[i].IsVisible) continue;
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