using OpenTK.Mathematics;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;

namespace OTK.LiteUI.Layouts
{
    public class VerticalSquareLayout : ILayout
    {
        [UIProperty]
        public float Padding
        {
            get;
            set;
        }

        [UIProperty]
        public int Columns
        {
            get;
            set;
        }

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