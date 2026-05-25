using OpenTK.Mathematics;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.UI.Components;

namespace OTK.LiteUI.Layouts
{
    public class VerticalGridLayout : ILayout
    {
        private int _columns;
        public float Padding
        {
            get;
            set;
        }

        public Vector2 ElementSize
        {
            get;
            set;
        }

        public VerticalGridLayout(int columns, Vector2 elementSize, float padding)
        {
            _columns = Math.Max(columns, 1);
            ElementSize = elementSize;
            Padding = padding;
        }

        public void Apply(Vector4 viewport, List<UIComponent> elements)
        {
            float startLeft = viewport.X;
            float startTop = viewport.W;
            float elementWidth = ((viewport.Z - viewport.X) - Padding * Math.Max(_columns - 1, 0)) / _columns;
            float elementHeight = ElementSize.Y;

            for (int i = 0; i < elements.Count; i++)
            {
                int column = i % _columns;
                int row = i / _columns;
                float left = startLeft + column * (elementWidth + Padding);
                float right = left + elementWidth;
                float top = startTop - row * (elementHeight + Padding);

                elements[i].Bounds = new Vector4(left, top - elementHeight, right, top);
            }
        }
    }
}