using OpenTK.Mathematics;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;

namespace OTK.LiteUI.Layouts
{
    public class GridLayout : ILayout
    {
        public float Padding { get; set; }
        public Vector2 ElementSize { get; set; }

        private int _rows;

        private int _columns;

        public GridLayout(int rows, int columns, float padding)
        {
            _rows = rows;
            _columns = columns;
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
                (totalWidth - Padding * (_columns - 1)) / _columns;

            float cellHeight =
                (totalHeight - Padding * (_rows - 1)) / _rows;

            for (int i = 0; i < elements.Count; i++)
            {
                int column = i % _columns;
                int row = i / _columns;

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