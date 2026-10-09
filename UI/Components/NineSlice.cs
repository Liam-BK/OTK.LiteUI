using OpenTK.Mathematics;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Rendering;
using OTK.LiteUI.UI.Utilities;

namespace OTK.LiteUI.UI.Components
{
    public class NineSlice : UIComponent, IRenderable
    {
        public string Texture { get; set; } = "";
        private float _inset;
        public float Inset
        {
            get => _inset;
            set
            {
                var min = Math.Min(Width, Height);
                if (min < value * 2)
                {
                    _inset = min * 0.5f;
                }
                else
                {
                    _inset = value;
                }
            }
        }
        private float _uvInset;
        public float UVInset
        {
            get
            {
                return _uvInset;
            }
            set
            {
                _uvInset = Math.Clamp(value, 0, 0.5f);
            }
        }
        private UIQuad topLeft = new UIQuad();
        private UIQuad topCenter = new UIQuad();
        private UIQuad topRight = new UIQuad();
        private UIQuad centerLeft = new UIQuad();
        private UIQuad center = new UIQuad();
        private UIQuad centerRight = new UIQuad();
        private UIQuad bottomLeft = new UIQuad();
        private UIQuad bottomCenter = new UIQuad();
        private UIQuad bottomRight = new UIQuad();

        public override Vector4 Colour
        {
            get => base.Colour;
            set
            {
                base.Colour = value;
                ForceUpdateQuadrantColours(value);
            }
        }

        public NineSlice(UICanvas canvas, Vector4 bounds, float inset = 10, float uvInset = 0.25f, Vector4? colour = null)
        {
            this.Canvas = canvas;
            Bounds = bounds;
            Inset = inset;
            UVInset = uvInset;
            Colour = colour ?? Vector4.One;
            UpdateQuadrants();
            canvas.Register(this);
            if (GetType() == typeof(NineSlice)) Identifier = Canvas.GetNextIdentifier(typeof(NineSlice));
        }

        protected void ForceUpdateQuadrantColours(Vector4 colour)
        {
            topLeft.colour = colour;
            topCenter.colour = colour;
            topRight.colour = colour;
            centerLeft.colour = colour;
            center.colour = colour;
            centerRight.colour = colour;
            bottomLeft.colour = colour;
            bottomCenter.colour = colour;
            bottomRight.colour = colour;
        }

        private void UpdateQuadrants()
        {
            int textureLayer;
            if (Texture == "") textureLayer = -1;
            else Canvas.TryGetTexture(Texture, out textureLayer);

            float left = Bounds.X;
            float innerLeft = Bounds.X + Inset;
            float innerRight = Bounds.Z - Inset;
            float right = Bounds.Z;

            float top = Bounds.W;
            float innerTop = Bounds.W - Inset;
            float innerBottom = Bounds.Y + Inset;
            float bottom = Bounds.Y;

            float edgeThickness = Inset;
            float innerWidth = Width - edgeThickness * 2;
            float innerHeight = Height - edgeThickness * 2;

            float leftEdge = Utils.GetMidPoint(left, innerLeft);
            float rightEdge = Utils.GetMidPoint(innerRight, right);

            float topEdge = Utils.GetMidPoint(top, innerTop);
            float bottomEdge = Utils.GetMidPoint(innerBottom, bottom);

            float middleX = Utils.GetMidPoint(innerLeft, innerRight);
            float middleY = Utils.GetMidPoint(innerTop, innerBottom);

            topLeft.position = new Vector2(leftEdge, topEdge);
            topLeft.size = new Vector2(edgeThickness, edgeThickness);
            topLeft.UVOffset = new Vector2(0, 1 - UVInset);
            topLeft.UVRange = new Vector2(UVInset, UVInset);
            topLeft.textureLayer = textureLayer;

            topCenter.position = new Vector2(middleX, topEdge);
            topCenter.size = new Vector2(innerWidth, edgeThickness);
            topCenter.UVOffset = new Vector2(UVInset, 1 - UVInset);
            topCenter.UVRange = new Vector2(1 - (UVInset * 2), UVInset);
            topCenter.textureLayer = textureLayer;

            topRight.position = new Vector2(rightEdge, topEdge);
            topRight.size = new Vector2(edgeThickness, edgeThickness);
            topRight.UVOffset = new Vector2(1 - UVInset, 1 - UVInset);
            topRight.UVRange = new Vector2(UVInset, UVInset);
            topRight.textureLayer = textureLayer;

            centerLeft.position = new Vector2(leftEdge, middleY);
            centerLeft.size = new Vector2(edgeThickness, innerHeight);
            centerLeft.UVOffset = new Vector2(0, UVInset);
            centerLeft.UVRange = new Vector2(UVInset, 1 - UVInset * 2);
            centerLeft.textureLayer = textureLayer;

            center.position = new Vector2(middleX, middleY);
            center.size = new Vector2(innerWidth, innerHeight);
            center.UVOffset = new Vector2(UVInset, UVInset);
            center.UVRange = new Vector2(1 - (UVInset * 2), 1 - UVInset * 2);
            center.textureLayer = textureLayer;

            centerRight.position = new Vector2(rightEdge, middleY);
            centerRight.size = new Vector2(edgeThickness, innerHeight);
            centerRight.UVOffset = new Vector2(1 - UVInset, UVInset);
            centerRight.UVRange = new Vector2(UVInset, 1 - UVInset * 2);
            centerRight.textureLayer = textureLayer;

            bottomLeft.position = new Vector2(leftEdge, bottomEdge);
            bottomLeft.size = new Vector2(edgeThickness, edgeThickness);
            bottomLeft.UVOffset = new Vector2(0, 0);
            bottomLeft.UVRange = new Vector2(UVInset, UVInset);
            bottomLeft.textureLayer = textureLayer;

            bottomCenter.position = new Vector2(middleX, bottomEdge);
            bottomCenter.size = new Vector2(innerWidth, edgeThickness);
            bottomCenter.UVOffset = new Vector2(UVInset, 0);
            bottomCenter.UVRange = new Vector2(1 - (UVInset * 2), UVInset);
            bottomCenter.textureLayer = textureLayer;

            bottomRight.position = new Vector2(rightEdge, bottomEdge);
            bottomRight.size = new Vector2(edgeThickness, edgeThickness);
            bottomRight.UVOffset = new Vector2(1 - UVInset, 0);
            bottomRight.UVRange = new Vector2(UVInset, UVInset);
            bottomRight.textureLayer = textureLayer;
        }

        public virtual void SubmitData(Renderer renderer)
        {
            if (!IsVisible) return;
            UpdateQuadrants();
            var visualOffset = Canvas.AnchorTypeOffsets[(int)Anchor](Canvas.CanvasSize, this);
            if (ClipBounds.HasValue)
            {
                var temp = Utils.Clip(topLeft, ClipBounds.Value);
                temp.position += visualOffset;
                renderer.AddInstance(temp);
                temp = Utils.Clip(topCenter, ClipBounds.Value);
                temp.position += visualOffset;
                renderer.AddInstance(temp);
                temp = Utils.Clip(topRight, ClipBounds.Value);
                temp.position += visualOffset;
                renderer.AddInstance(temp);
                temp = Utils.Clip(centerLeft, ClipBounds.Value);
                temp.position += visualOffset;
                renderer.AddInstance(temp);
                temp = Utils.Clip(center, ClipBounds.Value);
                temp.position += visualOffset;
                renderer.AddInstance(temp);
                temp = Utils.Clip(centerRight, ClipBounds.Value);
                temp.position += visualOffset;
                renderer.AddInstance(temp);
                temp = Utils.Clip(bottomLeft, ClipBounds.Value);
                temp.position += visualOffset;
                renderer.AddInstance(temp);
                temp = Utils.Clip(bottomCenter, ClipBounds.Value);
                temp.position += visualOffset;
                renderer.AddInstance(temp);
                temp = Utils.Clip(bottomRight, ClipBounds.Value);
                temp.position += visualOffset;
                renderer.AddInstance(temp);
            }
            else
            {
                var temp = topLeft;
                temp.position += visualOffset;
                renderer.AddInstance(temp);
                temp = topCenter;
                temp.position += visualOffset;
                renderer.AddInstance(temp);
                temp = topRight;
                temp.position += visualOffset;
                renderer.AddInstance(temp);
                temp = centerLeft;
                temp.position += visualOffset;
                renderer.AddInstance(temp);
                temp = center;
                temp.position += visualOffset;
                renderer.AddInstance(temp);
                temp = centerRight;
                temp.position += visualOffset;
                renderer.AddInstance(temp);
                temp = bottomLeft;
                temp.position += visualOffset;
                renderer.AddInstance(temp);
                temp = bottomCenter;
                temp.position += visualOffset;
                renderer.AddInstance(temp);
                temp = bottomRight;
                temp.position += visualOffset;
                renderer.AddInstance(temp);
            }
        }
    }
}