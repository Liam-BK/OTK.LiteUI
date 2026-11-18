using OpenTK.Mathematics;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.Core;

namespace OTK.LiteUI.Layouts
{
    public class VerticalGridLayout : ILayout
    {
        [UIProperty]
        public int Columns { get; set; }

        [UIProperty]
        public float Padding
        {
            get;
            set;
        }

        [UIProperty]
        public Vector2 ElementSize
        {
            get;
            set;
        }

        public VerticalGridLayout(int columns, Vector2 elementSize, float padding)
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
            float elementHeight = ElementSize.Y;

            for (int i = 0; i < elements.Count; i++)
            {
                if (!elements[i].IsVisible) continue;
                int column = i % Columns;
                int row = i / Columns;
                float left = startLeft + column * (elementWidth + Padding);
                float right = left + elementWidth;
                float top = startTop - row * (elementHeight + Padding);

                elements[i].Bounds = new Vector4(left, top - elementHeight, right, top);
            }
        }
    }
}