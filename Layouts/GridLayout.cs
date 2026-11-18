using OpenTK.Mathematics;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;

namespace OTK.LiteUI.Layouts
{
    public class GridLayout : ILayout
    {
        [UIProperty]
        public float Padding { get; set; }

        [UIProperty]
        public int Rows { get; set; }

        [UIProperty]
        public int Columns { get; set; }

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

            float cellWidth =
                (totalWidth - Padding * (Columns - 1)) / Columns;

            float cellHeight =
                (totalHeight - Padding * (Rows - 1)) / Rows;

            for (int i = 0; i < elements.Count; i++)
            {
                if (!elements[i].IsVisible) continue;
                int column = i % Columns;
                int row = i / Columns;

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