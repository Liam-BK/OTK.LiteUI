using OpenTK.Mathematics;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.Core;

namespace OTK.LiteUI.Layouts
{
    /// <summary>
    /// Arranges elements vertically from top to bottom, stretching each element to the available viewport width.
    /// </summary>
    public class VerticalLayout : ILayout
    {
        [UIProperty]
        public float Padding
        {
            get;
            set;
        }

        /// <summary>
        /// The height assigned to each element.
        /// </summary>
        [UIProperty]
        public float ElementHeight
        {
            get;
            set;
        }

        /// <summary>
        /// Creates a vertical layout with the specified element height and spacing.
        /// </summary>
        /// <param name="elementHeight">The height assigned to each element.</param>
        /// <param name="padding">The spacing between elements.</param>
        public VerticalLayout(float elementHeight, float padding)
        {
            ElementHeight = elementHeight;
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
                element.Bounds = new Vector4(left, top - ElementHeight, left + elementWidth, top);
                top -= ElementHeight + Padding;
            }
        }
    }
}