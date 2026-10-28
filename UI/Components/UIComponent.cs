using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.Core;


public enum ComponentOrientation
{
    Horizontal,
    Vertical
}

public enum AnchorType
{
    BottomLeft = 0,
    BottomCenter = 1,
    BottomRight = 2,
    MiddleLeft = 3,
    Center = 4,
    MiddleRight = 5,
    TopLeft = 6,
    TopCenter = 7,
    TopRight = 8
}

namespace OTK.LiteUI.UI.Components
{
    public abstract class UIComponent : UIElement
    {
        [UIProperty]
        public virtual AnchorType Anchor { get; set; } = AnchorType.BottomLeft;

        private IUIContainer? _parent = null;

        public virtual IUIContainer? Parent { get => _parent; set => _parent = value; }

        [UIProperty]
        public override bool IsVisible
        {
            get
            {
                return (Parent is not null && Parent is UIElement element) ? base.IsVisible && element.IsVisible : base.IsVisible;
            }
            set
            {
                base.IsVisible = value;
            }
        }

        public Page? TopLevelPage
        {
            get
            {
                var current = Parent;
                while (current is not null)
                {
                    if (current is Page page)
                        return page;

                    current = (current as UIComponent)?.Parent;
                }

                return null;
            }
        }

        private Vector4? _clipBounds = null;

        public virtual Vector4? ClipBounds
        {
            get
            {
                if (Parent is not UIComponent parent)
                    return _clipBounds;

                var parentClip = parent.ClipBounds;

                if (!parentClip.HasValue)
                    return _clipBounds;

                if (!_clipBounds.HasValue)
                    return parentClip;

                float left = Math.Max(parentClip.Value.X, _clipBounds.Value.X);
                float bottom = Math.Max(parentClip.Value.Y, _clipBounds.Value.Y);
                float right = Math.Min(parentClip.Value.Z, _clipBounds.Value.Z);
                float top = Math.Min(parentClip.Value.W, _clipBounds.Value.W);

                return new Vector4(left, bottom, right, top);
            }
            set
            {
                _clipBounds = value;
            }
        }

        private Vector4 _bounds = Vector4.Zero;

        [UIProperty]
        public virtual Vector4 Bounds
        {
            get
            {
                return _bounds;
            }
            set => _bounds = value;
        }

        public Vector2 Center => new Vector2((Bounds.X + Bounds.Z) * 0.5f, (Bounds.Y + Bounds.W) * 0.5f);

        public float Height => Bounds.W - Bounds.Y;

        public float Width => Bounds.Z - Bounds.X;

        protected Vector4 _colour = Vector4.One;

        [UIProperty]
        public virtual Vector4 Colour
        {
            get
            {
                return _colour;
            }
            set
            {
                _colour = new Vector4(Math.Clamp(value.X, 0, 1), Math.Clamp(value.Y, 0, 1), Math.Clamp(value.Z, 0, 1), Math.Clamp(value.W, 0, 1));
            }
        }

        public virtual bool CanFocus => false;

        public bool IsFocused => CanFocus && UICanvas.FocusedComponent == this;

        public virtual void OnFocusGained() { }

        public virtual void OnFocusLost() { }

        public virtual bool WithinBounds(MouseState mouse)
        {
            return WithinBounds(Canvas.ConvertMouseScreenCoords(mouse.Position));
        }

        public bool WithinBounds(Vector2 position)
        {
            var anchorPos = Canvas.AnchorTypeOffsets[(int)Anchor](Canvas.CanvasSize, this);
            return position.X > Bounds.X + anchorPos.X && position.X <= Bounds.Z + anchorPos.X && position.Y > Bounds.Y + anchorPos.Y && position.Y <= Bounds.W + anchorPos.Y;
        }
    }
}