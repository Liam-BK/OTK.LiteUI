using OpenTK.Mathematics;
using OTK.LiteUI.UI.Components;

namespace OTK.LiteUI.UI.Interfaces
{
    public interface ILayout
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

        public void Apply(Vector4 viewport, List<UIComponent> elements);
    }
}