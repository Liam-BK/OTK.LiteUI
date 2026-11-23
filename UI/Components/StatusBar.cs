using OpenTK.Mathematics;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Rendering;

namespace OTK.LiteUI.UI.Components
{
    /// <summary>
    /// A status bar that displays a proportional fill over a nine-slice background.
    /// </summary>
    public class StatusBar : NineSlice
    {
        private NineSlice Fill;

        /// <summary>
        /// Gets or sets the bounds of the status bar and its fill.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the anchor used by the status bar and its fill.
        /// </summary>
        [UIProperty]
        public override AnchorType Anchor
        {
            get => base.Anchor; set
            {
                base.Anchor = value;
                Fill.Anchor = value;
            }
        }

        /// <summary>
        /// Gets or sets the proportion of the status bar occupied by the fill.
        /// </summary>
        [UIProperty]
        public float FillAmount
        {
            get;
            set;
        }

        /// <summary>
        /// Sets the colour applied to the fill.
        /// </summary>
        [UIProperty]
        public Vector4 FillColour
        {
            set
            {
                Fill.Colour = value;
            }
        }

        /// <summary>
        /// Sets the texture used by the fill.
        /// </summary>
        [UIProperty]
        public string FillTexture
        {
            set
            {
                Fill.Texture = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the status bar and its fill are visible.
        /// </summary>
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

        /// <summary>
        /// Initializes a status bar on the specified canvas.
        /// </summary>
        /// <param name="canvas">The canvas to register the status bar with.</param>
        /// <param name="bounds">The bounds of the status bar.</param>
        /// <param name="inset">The corner inset of the background and fill.</param>
        /// <param name="uvInset">The texture coordinate inset of the background and fill.</param>
        /// <param name="colour">The initial colour of the background and fill.</param>
        public StatusBar(UICanvas canvas, Vector4 bounds, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(canvas, bounds, inset, uvInset, colour)
        {
            Fill = new NineSlice(canvas, bounds, inset, uvInset, colour);
            canvas.Deregister(Fill);
            if (GetType() == typeof(StatusBar)) Identifier = Canvas.GetNextIdentifier(typeof(StatusBar));
        }

        /// <summary>
        /// Initializes a status bar on the default canvas.
        /// </summary>
        /// <param name="bounds">The bounds of the status bar.</param>
        /// <param name="inset">The corner inset of the background and fill.</param>
        /// <param name="uvInset">The texture coordinate inset of the background and fill.</param>
        /// <param name="colour">The initial colour of the background and fill.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no default canvas is assigned.
        /// </exception>
        public StatusBar(Vector4 bounds, float inset = 10, float uvInset = 0.25f, Vector4? colour = null) : this(UICanvas.Default ?? throw new InvalidOperationException("Default Canvas must not be null for this Constructor. Try Creating a new UICanvas or assigning a UICanvas instance to UICanvas.Default"), bounds, inset, uvInset, colour) { }

        /// <summary>
        /// Submits the background and proportionally clipped fill to the renderer.
        /// </summary>
        /// <param name="renderer">The renderer receiving the status bar data.</param>
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