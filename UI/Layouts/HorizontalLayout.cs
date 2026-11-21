using OpenTK.Mathematics;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.Core;

namespace OTK.LiteUI.Layouts
{
    /// <summary>
    /// Arranges elements horizontally from left to right, maintaining a consistent element size.
    /// </summary>
    public class HorizontalLayout : ILayout
    {
        [UIProperty]
        public float Padding
        {
            get;
            set;
        }

        /// <summary>
        /// The size of each element in the viewport.
        /// </summary>
        [UIProperty]
        public Vector2 ElementSize
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a horizontal layout with the specified element size and spacing.
        /// </summary>
        /// <param name="elementSize">The width and height assigned to each element.</param>
        /// <param name="padding">The spacing between elements.</param>
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
                if (!element.IsVisible) continue;
                element.Bounds = new Vector4(left, top - ElementSize.Y, left + ElementSize.X, top);
                left += ElementSize.X + Padding;
            }
        }
    }
}