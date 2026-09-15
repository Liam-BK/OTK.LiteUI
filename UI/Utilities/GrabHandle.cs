using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.Core;

namespace OTK.LiteUI.UI.Utilities
{
    internal class GrabHandle : UIComponent
    {
        public bool Active = false;

        private Vector2 clickOffset = new();

        public GrabHandle(UICanvas canvas, Vector4 bounds)
        {
            this.Canvas = canvas;
            Bounds = bounds;
        }

        public override bool OnClickDown(MouseState mouse)
        {
            Active = WithinBounds(mouse) && IsVisible;
            var temp = Canvas.ConvertMouseScreenCoords(mouse.Position);
            clickOffset.X = Center.X - temp.X;
            clickOffset.Y = Center.Y - temp.Y;
            return Active;
        }

        public override bool OnMouseMove(MouseState mouse)
        {
            if (Active)
            {
                var convertedMouse = Canvas.ConvertMouseScreenCoords(mouse.Position);
                var x = convertedMouse.X + clickOffset.X;
                var y = convertedMouse.Y + clickOffset.Y;
                var halfWidth = Width * 0.5f;
                var halfHeight = Height * 0.5f;
                Bounds = new Vector4(x - halfWidth, y - halfHeight, x + halfWidth, y + halfHeight);
            }
            return Active;
        }

        public override bool OnClickUp(MouseState mouse)
        {
            Active = false;
            return false;
        }
    }
}