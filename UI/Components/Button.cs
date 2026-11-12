using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.UI.Rendering;

namespace OTK.LiteUI.UI.Components
{
    public class Button : NineSlice, ILinkable
    {
        protected Label label;
        [UIProperty]
        public string Text
        {
            get
            {
                return label is null ? "" : label.Text;
            }
            set
            {
                if (label is not null) label.Text = value;
            }
        }

        public Vector4 TextBounds
        {
            get
            {
                return label.Bounds;
            }
        }

        public float TimeToRollover = 0.2f;

        private float _rolloverValue = 0;

        private bool _isHovered = false;

        protected float RolloverValue
        {
            get => _rolloverValue;
            set => _rolloverValue = value;
        }

        [UIProperty]
        public override bool IsVisible
        {
            get => base.IsVisible;
            set
            {
                base.IsVisible = value;
                label.IsVisible = IsVisible;
                if (value == false) _rolloverValue = 0;
            }
        }

        [UIProperty]
        public override AnchorType Anchor
        {
            get => base.Anchor;
            set
            {
                base.Anchor = value;
                label.Anchor = value;
            }
        }

        private Vector4 _rolloverColour = new Vector4(0.75f, 0.75f, 0.75f, 1);

        [UIProperty]
        public Vector4 RolloverColour
        {
            set
            {
                _rolloverColour = value;
            }
        }

        [UIProperty]
        public Vector4 TextColour
        {
            set
            {
                label.Colour = value;
            }
        }

        public bool isPressed = false;

        private Vector4 _baseColour;

        [UIProperty]
        public override Vector4 Colour
        {
            get => _baseColour;
            set
            {
                _baseColour = value;
            }
        }

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
                if (label is not null)
                {
                    label.Origin = new Vector2((value.X + value.Z) * 0.5f, value.Y + (value.W - value.Y) * 0.25f);
                    label.Size = (value.W - value.Y) * 0.5f;
                    label.ClipBounds = value;
                }
            }
        }

        private string _pageLink = "";

        [UIProperty]
        public string PageLink
        {
            get
            {
                return _pageLink;
            }
            set
            {
                _pageLink = value;
            }
        }

        [UIProperty]
        public bool HideParentPageOnClick { get; set; } = true;

        public event Action<MouseButton>? OnPointerDown;

        public event Action<MouseButton>? OnClick;

        public event Action? OnHoverEnter;

        public event Action? OnHoverExit;

        public Button(UICanvas canvas, Vector4 bounds, string text = "", float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(canvas, bounds, inset, uvInset, colour)
        {
            _baseColour = colour ?? Vector4.One;
            label = new Label(canvas, new Vector2((bounds.X + bounds.Z) * 0.5f, bounds.Y + (bounds.W - bounds.Y) * 0.25f), (bounds.W - bounds.Y) * 0.5f, text);
            label.Alignment = TextAlignment.Center;
            label.Colour = new Vector4(0, 0, 0, 1);
            canvas.Deregister(label);
            if (GetType() == typeof(Button)) Identifier = Canvas.GetNextIdentifier(typeof(Button));
        }

        public Button(Vector4 bounds, string text = "", float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : this(UICanvas.Default ?? throw new InvalidOperationException("Default Canvas must not be null for this Constructor. Try Creating a new UICanvas or assigning a UICanvas instance to UICanvas.Default"), bounds, text, inset, uvInset, colour) { }

        public override bool OnClickDown(MouseState mouse)
        {
            if (!IsVisible) return false;
            isPressed = WithinBounds(mouse);
            if (ClipBounds is Vector4 clipBounds)
            {
                var anchorOffset = Canvas.AnchorTypeOffsets[(int)Anchor](Canvas.CanvasSize, this);
                var convertedMouse = Canvas.ConvertMouseScreenCoords(mouse.Position) - anchorOffset;
                isPressed &= convertedMouse.X >= clipBounds.X && convertedMouse.X <= clipBounds.Z && convertedMouse.Y >= clipBounds.Y && convertedMouse.Y <= clipBounds.W;
            }

            if (isPressed)
            {
                if (mouse.IsButtonDown(MouseButton.Left)) OnPointerDown?.Invoke(MouseButton.Left);
                else if (mouse.IsButtonDown(MouseButton.Right)) OnPointerDown?.Invoke(MouseButton.Right);
            }
            return isPressed;
        }

        public override bool OnMouseMove(MouseState mouse)
        {
            if (!IsVisible) return false;
            var nextHoverState = WithinBounds(mouse);
            if (ClipBounds is Vector4 clipBounds)
            {
                var anchorOffset = Canvas.AnchorTypeOffsets[(int)Anchor](Canvas.CanvasSize, this);
                var convertedMouse = Canvas.ConvertMouseScreenCoords(mouse.Position) - anchorOffset;
                nextHoverState &= convertedMouse.X >= clipBounds.X && convertedMouse.X <= clipBounds.Z && convertedMouse.Y >= clipBounds.Y && convertedMouse.Y <= clipBounds.W;
            }
            if (nextHoverState != _isHovered)
            {
                if (nextHoverState == false)
                {
                    OnHoverExit?.Invoke();
                }
                else if (nextHoverState == true)
                {
                    OnHoverEnter?.Invoke();
                }
                _isHovered = nextHoverState;
            }
            if (isPressed)
            {
                isPressed = WithinBounds(mouse);
            }
            return isPressed;
        }

        public override bool OnClickUp(MouseState mouse)
        {
            if (!IsVisible) return false;
            if (isPressed && WithinBounds(mouse))
            {
                if (mouse.IsButtonReleased(MouseButton.Left))
                {
                    OnClick?.Invoke(MouseButton.Left);
                    var page = Canvas.Get<Page>(PageLink);
                    if (page is not null)
                    {
                        page.IsVisible = true;
                        if (HideParentPageOnClick)
                        {
                            var parentPage = TopLevelPage;
                            if (parentPage is not null && parentPage != page)
                            {
                                parentPage.IsVisible = false;
                            }
                        }
                    }
                }
                else if (mouse.IsButtonReleased(MouseButton.Right))
                {
                    OnClick?.Invoke(MouseButton.Right);
                }

                isPressed = false;
                return true;
            }
            return false;
        }

        public override void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
        {
            if (!IsVisible) return;
            var isHovered = WithinBounds(mouse);

            if (ClipBounds is Vector4 clipBounds)
            {
                var anchorOffset = Canvas.AnchorTypeOffsets[(int)Anchor](Canvas.CanvasSize, this);
                var convertedMouse = Canvas.ConvertMouseScreenCoords(mouse.Position) - anchorOffset;
                isHovered &= convertedMouse.X >= clipBounds.X && convertedMouse.X <= clipBounds.Z && convertedMouse.Y >= clipBounds.Y && convertedMouse.Y <= clipBounds.W;
            }
            var multiplier = new Vector4(isPressed ? 0.5f : 1.0f, isPressed ? 0.5f : 1.0f, isPressed ? 0.5f : 1.0f, 1);
            if (TimeToRollover == 0) RolloverValue = isHovered ? 1 : 0;
            else RolloverValue = Math.Clamp(RolloverValue + deltaTime * (isHovered ? 1 : -1) / TimeToRollover, 0, 1);
            var lerpColour = Vector4.Lerp(_baseColour, _rolloverColour, Math.Clamp(RolloverValue, 0, 1));
            ForceUpdateQuadrantColours(lerpColour * multiplier);
        }

        public override void SubmitData(Renderer renderer)
        {
            if (!IsVisible || Colour.W <= 0) return;
            base.SubmitData(renderer);
            var clippingArea = ClipBounds;
            if (!(clippingArea is not null)) label.ClipBounds = Bounds;
            else label.ClipBounds = new Vector4(Math.Max(clippingArea.Value.X, Bounds.X), Math.Max(clippingArea.Value.Y, Bounds.Y), Math.Min(clippingArea.Value.Z, Bounds.Z), Math.Min(clippingArea.Value.W, Bounds.W));
            label.SubmitData(renderer);
        }
    }
}