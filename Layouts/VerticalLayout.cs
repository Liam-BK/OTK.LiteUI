using OpenTK.Mathematics;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.Core;

namespace OTK.LiteUI.Layouts
{
    public class VerticalLayout : ILayout
    {
        [UIProperty]
        public float Padding
        {
            get;
            set;
        }

        [UIProperty]
        public float ElementSize
        {
            get;
            set;
        }

        public VerticalLayout(float elementSize, float padding)
        {
            ElementSize = elementSize;
            Padding = padding;
        }

        public void Apply(Vector4 viewport, List<UIComponent> elements)
        {
            float left = viewport.X;
            float top = viewport.W;
            float elementWidth = Math.Max(viewport.Z - viewport.X, 0);
            foreach (var element in elements)
            {
                if (!element.IsVisible) continue;
                element.Bounds = new Vector4(left, top - ElementSize, left + elementWidth, top);
                top -= ElementSize + Padding;
            }
        }
    }
}