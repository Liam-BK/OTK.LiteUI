using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
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
    public abstract class UIComponent
    {
        public UICanvas Canvas = null!;

        public virtual AnchorType Anchor { get; set; } = AnchorType.BottomLeft;

        public virtual bool IsVisible { get; set; } = true;

        private IUIContainer? _parent = null;

        public IUIContainer? Parent { get => _parent; set => _parent = value; }

        private Vector4? _clipBounds = null;
        public string Identifier
        {
            get;
            set;
        } = "";

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

        public virtual void OnFocusGained() { }

        public virtual void OnFocusLost() { }

        public bool IsFocused => CanFocus && UICanvas.FocusedComponent is not null;

        public virtual void Deregister(List<UIComponent> registry)
        {
            if (!registry.Remove(this)) throw new InvalidOperationException("Attempted to deregister a UI element that was not registered.");
        }

        public virtual bool OnClickDown(MouseState mouse)
        {
            return false;
        }

        public virtual bool OnClickUp(MouseState mouse)
        {
            return false;
        }

        public virtual bool OnMouseMove(MouseState mouse)
        {
            return false;
        }

        public virtual bool OnMouseWheel(MouseState mouse)
        {
            return false;
        }

        public virtual void OnTextInput(TextInputEventArgs e)
        {

        }

        public virtual void OnKeyDown(KeyboardKeyEventArgs e)
        {

        }

        public virtual void OnKeyUp(KeyboardKeyEventArgs e)
        {

        }

        public virtual void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
        {

        }

        public virtual bool WithinBounds(MouseState mouse)
        {
            return WithinBounds(Canvas.ConvertMouseScreenCoords(mouse.Position));
        }

        public bool WithinBounds(Vector2 position)
        {
            var anchorPos = Canvas.AnchorTypeOffsets[(int)Anchor](Canvas.CanvasSize, this);
            return position.X > Bounds.X + anchorPos.X && position.X <= Bounds.Z + anchorPos.X && position.Y > Bounds.Y + anchorPos.Y && position.Y <= Bounds.W + anchorPos.Y;
        }

        public static float MapValue(float value, float inputMin, float inputMax, float outputMin, float outputMax)
        {
            if (Math.Abs(inputMax - inputMin) < 0.0005f) return inputMin;
            return outputMin + (value - inputMin) * (outputMax - outputMin) / (inputMax - inputMin);
        }
    }
}