using OpenTK.Mathematics;

namespace OTK.LiteUI.UI.Interfaces
{
    public interface IScrollable
    {
        public Vector4 ViewPort
        {
            get;
        }

        public Vector2 ScrollOffset
        {
            get;
            set;
        }

        public Vector2 MaxScroll
        {
            get;
        }
    }
}