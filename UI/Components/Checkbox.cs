using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace OTK.LiteUI.UI.Components
{
    public class Checkbox : NineSlice
    {
        public bool _isPressed = false;

        public string UncheckedTexture = "";

        public string CheckedTexture = "";

        public Vector4 UncheckedColour;

        public Vector4 CheckedColour;

        public bool Checked
        {
            get;
            set;
        }

        public override Vector4 Bounds
        {
            get => base.Bounds;
            set
            {
                var result = new Vector4(value.Z - (value.W - value.Y), value.Y, value.Z, value.W);
                base.Bounds = result;
            }
        }

        public event Action<MouseButton>? OnClick;

        public Checkbox(Vector4 bounds, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(bounds, inset, uvInset, colour)
        {
            UncheckedColour = colour ?? Vector4.One;
            CheckedColour = colour ?? Vector4.One;
        }

        public override bool OnClickDown(MouseState mouse)
        {
            if (!IsVisible) return false;
            _isPressed = WithinBounds(mouse);

            return _isPressed;
        }

        public override bool OnMouseMove(MouseState mouse)
        {
            if (!IsVisible) return false;
            if (_isPressed)
            {
                _isPressed = WithinBounds(mouse);
            }
            return _isPressed;
        }

        public override bool OnClickUp(MouseState mouse)
        {
            if (!IsVisible) return false;
            if (_isPressed && WithinBounds(mouse))
            {
                Checked = !Checked;
                if (mouse.IsButtonReleased(MouseButton.Left))
                {
                    OnClick?.Invoke(MouseButton.Left);
                }
                else if (mouse.IsButtonReleased(MouseButton.Right))
                {
                    OnClick?.Invoke(MouseButton.Right);
                }
                _isPressed = false;
                return true;
            }
            return false;
        }

        public override void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
        {
            if (!IsVisible) return;
            var multiplier = new Vector4(_isPressed ? 0.5f : 1.0f, _isPressed ? 0.5f : 1.0f, _isPressed ? 0.5f : 1.0f, 1);
            Texture = Checked ? CheckedTexture : UncheckedTexture;
            Colour = (Checked ? CheckedColour : UncheckedColour) * multiplier;
        }
    }
}