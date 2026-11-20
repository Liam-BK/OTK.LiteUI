using OpenTK.Mathematics;
using OTK.LiteUI.UI.Components;

namespace OTK.LiteUI.UI.Interfaces
{
    /// <summary>
    /// Defines how UI components are arranged within a container.
    /// </summary>
    public interface ILayout
    {
        /// <summary>
        /// The spacing applied between elements.
        /// </summary>
        public float Padding
        {
            get;
            set;
        }

        /// <summary>
        /// Arranges the specified elements within the given viewport.
        /// </summary>
        /// <param name="viewport">The bounds available for the layout.</param>
        /// <param name="elements">The elements to arrange.</param>
        public void Apply(Vector4 viewport, List<UIComponent> elements);
    }
}