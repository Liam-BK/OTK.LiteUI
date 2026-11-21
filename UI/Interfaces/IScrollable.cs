using OpenTK.Mathematics;

namespace OTK.LiteUI.UI.Interfaces
{
    /// <summary>
    /// Defines a component that supports scrolling within a viewport.
    /// </summary>
    public interface IScrollable
    {
        /// <summary>
        /// The bounds of the visible scrolling area.
        /// </summary>
        public Vector4 ViewPort
        {
            get;
        }

        /// <summary>
        /// The current horizontal and vertical scroll offset.
        /// </summary>
        public Vector2 ScrollOffset
        {
            get;
            set;
        }

        /// <summary>
        /// The maximum horizontal and vertical scroll offsets.
        /// </summary>
        public Vector2 MaxScroll
        {
            get;
        }
    }
}