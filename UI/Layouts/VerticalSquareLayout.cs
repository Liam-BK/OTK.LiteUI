using OpenTK.Mathematics;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;

namespace OTK.LiteUI.Layouts
{
    /// <summary>
    /// Arranges elements in a vertical grid of square cells, filling each row from left to right before moving to the next.
    /// </summary>
    public class VerticalSquareLayout : ILayout
    {
        [UIProperty]
        public float Padding
        {
            get;
            set;
        }

        /// <summary>
        /// The number of columns in the layout. Values are clamped to a minimum of one.
        /// </summary>
        [UIProperty]
        public int Columns
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a vertical square layout with the specified number of columns and spacing.
        /// </summary>
        /// <param name="columns">The number of columns in the layout. Values below one are clamped to one.</param>
        /// <param name="padding">The spacing between elements.</param>
        public VerticalSquareLayout(int columns, float padding)
        {
            Columns = Math.Max(columns, 1);
            Padding = padding;
        }

        public void Apply(Vector4 viewport, List<UIComponent> elements)
        {
            float startLeft = viewport.X;
            float startTop = viewport.W;
            float elementWidth = (viewport.Z - viewport.X - Padding * Math.Max(Columns - 1, 0)) / Columns;
            float elementHeight = elementWidth;
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
                float left = startLeft + column * (elementWidth + Padding);
                float right = left + elementWidth;
                float top = startTop - row * (elementHeight + Padding);

                elements[i].Bounds = new Vector4(left, top - elementHeight, right, top);
            }
        }
    }
}