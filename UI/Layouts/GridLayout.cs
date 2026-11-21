using OpenTK.Mathematics;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;

namespace OTK.LiteUI.Layouts
{
    /// <summary>
    /// Arranges elements in a uniform grid, filling each row from left to right before moving to the next.
    /// </summary>
    public class GridLayout : ILayout
    {
        [UIProperty]
        public float Padding { get; set; }

        /// <summary>
        /// The number of rows that fit exactly within the provided viewport vertically
        /// </summary>
        [UIProperty]
        public int Rows { get; set; }

        /// <summary>
        /// The number of columns that fit exactly within the provided viewport horizontally
        /// </summary>
        [UIProperty]
        public int Columns { get; set; }

        /// <summary>
        /// Creates a grid layout with the specified number of rows and columns.
        /// </summary>
        /// <param name="rows">The number of rows that fit exactly within the provided viewport vertically.</param>
        /// <param name="columns">The number of columns that fit exactly within the provided viewport horizontally.</param>
        /// <param name="padding">The spacing between elements and rows.</param>
        public GridLayout(int rows, int columns, float padding)
        {
            Rows = rows;
            Columns = columns;
            Padding = padding;
        }

        public void Apply(Vector4 viewport, List<UIComponent> elements)
        {
            float left = viewport.X;
            float right = viewport.Z;
            float top = viewport.W;
            float bottom = viewport.Y;

            float totalWidth = right - left;
            float totalHeight = top - bottom;

            float cellWidth = (totalWidth - Padding * (Columns - 1)) / Columns;
            float cellHeight = (totalHeight - Padding * (Rows - 1)) / Rows;

            int skipCount = 0;

            for (int i = 0; i < elements.Count; i++)
            {
                if (!elements[i].IsVisible)
                {
                    skipCount++;
                    continue;
                }
                int column = (i - skipCount) % Columns;
                int row = (i - skipCount) / Columns;

                float x = left + column * (cellWidth + Padding);
                float y = top - row * (cellHeight + Padding);

                float elementLeft = x;
                float elementRight = x + cellWidth;

                float elementTop = y;
                float elementBottom = y - cellHeight;

                elements[i].Bounds = new Vector4(
                    elementLeft,
                    elementBottom,
                    elementRight,
                    elementTop
                );
            }
        }
    }
}