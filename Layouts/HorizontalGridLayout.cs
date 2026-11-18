using OpenTK.Mathematics;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.Core;

namespace OTK.LiteUI.Layouts
{
    public class HorizontalGridLayout : ILayout
    {
        [UIProperty]
        public int Rows { get; set; }

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

        public HorizontalGridLayout(int rows, Vector2 elementSize, float padding)
        {
            Rows = Math.Max(rows, 1);
            ElementSize = elementSize;
            Padding = padding;
        }

        public void Apply(Vector4 viewport, List<UIComponent> elements)
        {
            float startLeft = viewport.X;
            float startTop = viewport.W;
            float elementHeight = ((viewport.W - viewport.Y) - Padding * Math.Max(Rows - 1, 0)) / Rows;
            float elementWidth = ElementSize.X;

            for (int i = 0; i < elements.Count; i++)
            {
                if (!elements[i].IsVisible) continue;
                int column = i / Rows;
                int row = i % Rows;
                float left = startLeft + column * (elementWidth + Padding);
                float right = left + elementWidth;
                float top = startTop - row * (elementHeight + Padding);

                elements[i].Bounds = new Vector4(left, top - elementHeight, right, top);
            }
        }
    }
}