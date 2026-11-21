using OpenTK.Mathematics;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.Core;

namespace OTK.LiteUI.Layouts
{
    /// <summary>
    /// Arranges elements in a vertical grid, filling each row from left to right before moving to the next.
    /// </summary>
    public class VerticalGridLayout : ILayout
    {
        [UIProperty]
        public float Padding
        {
            get;
            set;
        }

        /// <summary>
        /// The size used for each element's height. The width is calculated from the available viewport width.
        /// </summary>
        [UIProperty]
        public float ElementSize
        {
            get;
            set;
        }

        /// <summary>
        /// The number of columns in the layout. Values are clamped to a minimum of one.
        /// </summary>
        [UIProperty]
        public int Columns { get; set; }

        /// <summary>
        /// Creates a vertical grid layout with the specified number of columns, element size, and spacing.
        /// </summary>
        /// <param name="columns">The number of columns in the layout. Values below one are clamped to one.</param>
        /// <param name="elementSize">The size used for each element's height.</param>
        /// <param name="padding">The spacing between elements.</param>
        public VerticalGridLayout(int columns, float elementSize, float padding)
        {
            Columns = Math.Max(columns, 1);
            ElementSize = elementSize;
            Padding = padding;
        }

        public void Apply(Vector4 viewport, List<UIComponent> elements)
        {
            float startLeft = viewport.X;
            float startTop = viewport.W;
            float elementWidth = ((viewport.Z - viewport.X) - Padding * Math.Max(Columns - 1, 0)) / Columns;
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
                float top = startTop - row * (ElementSize + Padding);

                elements[i].Bounds = new Vector4(left, top - ElementSize, right, top);
            }
        }
    }
}