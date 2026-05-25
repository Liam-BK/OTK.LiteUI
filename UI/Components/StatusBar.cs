using OpenTK.Mathematics;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Rendering;

namespace OTK.LiteUI.UI.Components
{
    public class StatusBar : NineSlice
    {
        private NineSlice Fill;

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

        public float FillAmount
        {
            get;
            set;
        }

        public Vector4 FillColour
        {
            set
            {
                Fill.Colour = value;
            }
        }

        public string FillTexture
        {
            set
            {
                Fill.Texture = value;
            }
        }

        public StatusBar(Vector4 bounds, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(bounds, inset, uvInset, colour)
        {
            Fill = new NineSlice(bounds, inset, uvInset, colour);
            UIScene.Deregister(Fill);
        }

        public override void SubmitData(Renderer renderer)
        {
            base.SubmitData(renderer);
            var fillClipBounds = new Vector4(Bounds.X, Bounds.Y, Bounds.X + FillAmount * Width, Bounds.W);
            if (ClipBounds.HasValue) fillClipBounds = new Vector4(Math.Max(fillClipBounds.X, ClipBounds.Value.X), Math.Max(fillClipBounds.Y, ClipBounds.Value.Y), Math.Min(fillClipBounds.Z, ClipBounds.Value.Z), Math.Min(fillClipBounds.W, ClipBounds.Value.W));
            Fill.ClipBounds = fillClipBounds;
            Fill.SubmitData(renderer);
        }
    }
}