using OpenTK.Mathematics;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.Core;


namespace OTK.LiteUI.Layouts
{
    public class FlowLayout : ILayout
    {
        [UIProperty]
        public float Padding
        {
            get;
            set;
        }

        /// <summary>
        /// The width and height assigned to each element.
        /// </summary>
        [UIProperty]
        public Vector2 ElementSize
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a flow layout with the specified element size and spacing.
        /// </summary>
        /// <param name="elementSize">The width and height of each element.</param>
        /// <param name="padding">The spacing between elements and rows.</param>
        public FlowLayout(Vector2 elementSize, float padding)
        {
            ElementSize = elementSize;
            Padding = padding;
        }

        public void Apply(Vector4 viewport, List<UIComponent> elements)
        {
            float left = viewport.X;
            float top = viewport.W;
            float elementWidth = ElementSize.X;
            float elementHeight = ElementSize.Y;
            foreach (var element in elements)
            {
                if (!element.IsVisible) continue;
                if (left + elementWidth > viewport.Z - Padding && left != viewport.X)
                {
                    left = viewport.X;
                    top -= elementHeight + Padding;
                }

                element.Bounds = new Vector4(
                    left,
                    top - elementHeight,
                    left + elementWidth,
                    top
                );
                left += elementWidth + Padding;
            }
        }
    }
}