using OpenTK.Mathematics;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.Core;

namespace OTK.LiteUI.Layouts
{
    /// <summary>
    /// Arranges elements in a horizontal grid, filling each column from top to bottom before moving to the next.
    /// </summary>
    public class HorizontalGridLayout : ILayout
    {
        [UIProperty]
        public float Padding
        {
            get;
            set;
        }

        /// <summary>
        /// The width of each element in the viewport.
        /// </summary>
        [UIProperty]
        public float ElementWidth
        {
            get;
            set;
        }

        /// <summary>
        /// The number of rows that fit exactly within the provided viewport vertically.
        /// </summary>
        [UIProperty]
        public int Rows { get; set; }

        /// <summary>
        /// Creates a horizontal grid layout with the specified number of rows, element width, and spacing.
        /// </summary>
        /// <param name="rows">The number of rows in the grid. Must be at least one.</param>
        /// <param name="elementWidth">The width assigned to each element.</param>
        /// <param name="padding">The spacing between elements.</param>
        public HorizontalGridLayout(int rows, float elementWidth, float padding)
        {
            Rows = Math.Max(rows, 1);
            ElementWidth = elementWidth;
            Padding = padding;
        }

        public void Apply(Vector4 viewport, List<UIComponent> elements)
        {
            float startLeft = viewport.X;
            float startTop = viewport.W;
            float elementHeight = ((viewport.W - viewport.Y) - Padding * Math.Max(Rows - 1, 0)) / Rows;
            int skipCount = 0;

            for (int i = 0; i < elements.Count; i++)
            {
                if (!elements[i].IsVisible)
                {
                    skipCount++;
                    continue;
                }
                int column = (i - skipCount) / Rows;
                int row = (i - skipCount) % Rows;
                float left = startLeft + column * (ElementWidth + Padding);
                float right = left + ElementWidth;
                float top = startTop - row * (elementHeight + Padding);

                elements[i].Bounds = new Vector4(left, top - elementHeight, right, top);
            }
        }
    }
}