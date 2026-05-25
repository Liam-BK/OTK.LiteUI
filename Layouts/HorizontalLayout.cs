using OpenTK.Mathematics;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.UI.Components;

namespace OTK.LiteUI.Layouts
{
    public class HorizontalLayout : ILayout
    {
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

        public HorizontalLayout(Vector2 elementSize, float padding)
        {
            ElementSize = elementSize;
            Padding = padding;
        }

        public void Apply(Vector4 viewport, List<UIComponent> elements)
        {
            float left = viewport.X;
            float top = viewport.W;
            foreach (var element in elements)
            {
                element.Bounds = new Vector4(left, top - ElementSize.Y, left + ElementSize.X, top);
                left += ElementSize.X + Padding;
            }
        }
    }
}