using OpenTK.Mathematics;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;

namespace OTK.LiteUI.Layouts
{
    public class HorizontalSquareLayout : ILayout
    {
        [UIProperty]
        public float Padding
        {
            get;
            set;
        }

        [UIProperty]
        public int Rows { get; set; }

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