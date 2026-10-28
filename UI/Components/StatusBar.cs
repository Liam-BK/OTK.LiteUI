using OpenTK.Mathematics;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Rendering;

namespace OTK.LiteUI.UI.Components
{
    public class StatusBar : NineSlice
    {
        private NineSlice Fill;

        [UIProperty]
        public override Vector4 Bounds
        {
            get
            {
                return base.Bounds;
            }
            set
            {
                base.Bounds = value;
                if (Fill is not null) Fill.Bounds = value;
            }
        }

        [UIProperty]
        public override AnchorType Anchor
        {
            get => base.Anchor; set
            {
                base.Anchor = value;
                Fill.Anchor = value;
            }
        }

        [UIProperty]
        public float FillAmount
        {
            get;
            set;
        }

        [UIProperty]
        public Vector4 FillColour
        {
            set
            {
                Fill.Colour = value;
            }
        }

        [UIProperty]
        public string FillTexture
        {
            set
            {
                Fill.Texture = value;
            }
        }

        [UIProperty]
        public override bool IsVisible
        {
            get => base.IsVisible;
            set
            {
                base.IsVisible = value;
                Fill.IsVisible = value;
            }
        }

        public StatusBar(UICanvas canvas, Vector4 bounds, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(canvas, bounds, inset, uvInset, colour)
        {
            Fill = new NineSlice(canvas, bounds, inset, uvInset, colour);
            canvas.Deregister(Fill);
            if (GetType() == typeof(StatusBar)) Identifier = Canvas.GetNextIdentifier(typeof(StatusBar));
        }

        public override void SubmitData(Renderer renderer)
        {
            if (!IsVisible) return;
            if (Colour.W > 0) base.SubmitData(renderer);
            if (Fill.Colour.W <= 0) return;
            var fillClipBounds = new Vector4(Bounds.X, Bounds.Y, Bounds.X + FillAmount * Width, Bounds.W);
            if (ClipBounds.HasValue)
            {
                var clippingArea = ClipBounds.Value;
                fillClipBounds = new Vector4(Math.Max(fillClipBounds.X, clippingArea.X), Math.Max(fillClipBounds.Y, clippingArea.Y), Math.Min(fillClipBounds.Z, clippingArea.Z), Math.Min(fillClipBounds.W, clippingArea.W));
            }
            Fill.ClipBounds = fillClipBounds;
            Fill.SubmitData(renderer);
        }
    }
}