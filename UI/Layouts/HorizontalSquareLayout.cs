using OpenTK.Mathematics;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;

namespace OTK.LiteUI.Layouts
{
    /// <summary>
    /// Arranges elements in a horizontal grid of square cells, filling each column from top to bottom before moving to the next.
    /// </summary>
    public class HorizontalSquareLayout : ILayout
    {
        [UIProperty]
        public float Padding
        {
            get;
            set;
        }

        /// <summary>
        /// The number of rows in the layout. Values are clamped to a minimum of one.
        /// </summary>
        [UIProperty]
        public int Rows { get; set; }

        /// <summary>
        /// Creates a horizontal square layout with the specified number of rows and spacing.
        /// </summary>
        /// <param name="rows">The number of rows in the layout. Values below one are clamped to one.</param>
        /// <param name="padding">The spacing between elements.</param>
        public HorizontalSquareLayout(int rows, float padding)
        {
            Rows = Math.Max(rows, 1);
            Padding = padding;
        }

        public void Apply(Vector4 viewport, List<UIComponent> elements)
        {
            float startLeft = viewport.X;
            float startTop = viewport.W;
            float elementHeight = (viewport.W - viewport.Y - Padding * Math.Max(Rows - 1, 0)) / Rows;
            float elementWidth = elementHeight;
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
                float left = startLeft + column * (elementWidth + Padding);
                float right = left + elementWidth;
                float top = startTop - row * (elementHeight + Padding);

                elements[i].Bounds = new Vector4(left, top - elementHeight, right, top);
            }
        }
    }
}