using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OTK.LiteUI.Core;

namespace OTK.LiteUI.UI.Components
{
    public class Checkbox : NineSlice
    {
        private bool _isPressed = false;

        [UIProperty]
        public string UncheckedTexture { get; set; } = "";

        [UIProperty]
        public string CheckedTexture { get; set; } = "";

        [UIProperty]
        public Vector4 UncheckedColour { get; set; }

        [UIProperty]
        public Vector4 CheckedColour { get; set; }

        [UIProperty]
        public bool Checked
        {
            get;
            set;
        }

        [UIProperty]
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

        public Checkbox(UICanvas canvas, Vector4 bounds, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(canvas, bounds, inset, uvInset, colour)
        {
            this.Canvas = canvas;
            UncheckedColour = colour ?? Vector4.One;
            CheckedColour = colour ?? Vector4.One;
            if (GetType() == typeof(Checkbox)) Identifier = Canvas.GetNextIdentifier(typeof(Checkbox));
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
            if (!IsVisible || Colour.W <= 0) return;
            var multiplier = new Vector4(_isPressed ? 0.5f : 1.0f, _isPressed ? 0.5f : 1.0f, _isPressed ? 0.5f : 1.0f, 1);
            Texture = Checked ? CheckedTexture : UncheckedTexture;
            Colour = (Checked ? CheckedColour : UncheckedColour) * multiplier;
        }
    }
}