using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.UI.Rendering;

namespace OTK.LiteUI.UI.Components
{
    /// <summary>
    /// A clickable UI component with a nine-slice background, text label, hover effects, and optional page navigation.
    /// </summary>
    public class Button : NineSlice, ILinkable
    {
        /// <summary>
        /// The label displayed within the button.
        /// </summary>
        protected Label label;

        /// <summary>
        /// The text displayed on the button.
        /// </summary>
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

        /// <summary>
        /// The bounds of the button's text label.
        /// </summary>
        public Vector4 TextBounds
        {
            get
            {
                return label.Bounds;
            }
        }

        /// <summary>
        /// The duration, in seconds, of the button's hover colour transition.
        /// </summary>
        public float TimeToRollover = 0.2f;

        private float _rolloverValue = 0;

        private bool _isHovered = false;

        /// <summary>
        /// The current progress of the hover colour transition, ranging from 0 to 1.
        /// </summary>
        protected float RolloverValue
        {
            get => _rolloverValue;
            set => _rolloverValue = value;
        }

        /// <summary>
        /// Gets or sets whether the button is visible.
        /// Also updates the visibility of its text label.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the button's anchor.
        /// The text label's anchor is updated to match.
        /// </summary>
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

        /// <summary>
        /// Sets the colour used when the button is hovered.
        /// </summary>
        [UIProperty]
        public Vector4 RolloverColour
        {
            set
            {
                _rolloverColour = value;
            }
        }

        /// <summary>
        /// Sets the colour of the button's text.
        /// </summary>
        [UIProperty]
        public Vector4 TextColour
        {
            set
            {
                label.Colour = value;
            }
        }

        /// <summary>
        /// Whether the button is currently pressed.
        /// </summary>
        public bool isPressed = false;

        private Vector4 _baseColour;

        /// <summary>
        /// Gets or sets the button's base colour.
        /// </summary>
        [UIProperty]
        public override Vector4 Colour
        {
            get => _baseColour;
            set
            {
                _baseColour = value;
            }
        }

        /// <summary>
        /// Gets or sets the button's bounds.
        /// Updating the bounds also repositions and resizes its text label.
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
                if (label is not null)
                {
                    label.Origin = new Vector2((value.X + value.Z) * 0.5f, value.Y + (value.W - value.Y) * 0.25f);
                    label.Size = (value.W - value.Y) * 0.5f;
                    label.ClipBounds = value;
                }
            }
        }

        private string _pageLink = "";

        /// <summary>
        /// Gets or sets the identifier of the page to set visible when clicked.
        /// </summary>
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

        /// <summary>
        /// Whether the parent page should be hidden when the button navigates to another page.
        /// Defaults to true.
        /// </summary>
        [UIProperty]
        public bool HideParentPageOnClick { get; set; } = true;

        /// <summary>
        /// Occurs when a mouse button is pressed while the pointer is over the button.
        /// </summary>
        public event Action<MouseButton>? OnPointerDown;

        /// <summary>
        /// Occurs when a mouse button is released while the button is pressed.
        /// </summary>
        public event Action<MouseButton>? OnClick;

        /// <summary>
        /// Occurs when the pointer enters the button's bounds.
        /// </summary>
        public event Action? OnHoverEnter;

        /// <summary>
        /// Occurs when the pointer leaves the button's bounds.
        /// </summary>
        public event Action? OnHoverExit;

        /// <summary>
        /// Initializes a button on the specified canvas.
        /// </summary>
        /// <param name="canvas">The canvas to register the button with.</param>
        /// <param name="bounds">The bounds of the button.</param>
        /// <param name="text">The text displayed on the button.</param>
        /// <param name="inset">The nine-slice inset.</param>
        /// <param name="uvInset">The texture coordinate inset.</param>
        /// <param name="colour">The button's initial colour. Defaults to white.</param>
        public Button(UICanvas canvas, Vector4 bounds, string text = "", float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(canvas, bounds, inset, uvInset, colour)
        {
            _baseColour = colour ?? Vector4.One;
            label = new Label(canvas, new Vector2((bounds.X + bounds.Z) * 0.5f, bounds.Y + (bounds.W - bounds.Y) * 0.25f), (bounds.W - bounds.Y) * 0.5f, text);
            label.Alignment = TextAlignment.Center;
            label.Colour = new Vector4(0, 0, 0, 1);
            canvas.Deregister(label);
            if (GetType() == typeof(Button)) Identifier = Canvas.GetNextIdentifier(typeof(Button));
        }

        /// <summary>
        /// Initializes a button on the default canvas.
        /// </summary>
        /// <param name="bounds">The bounds of the button.</param>
        /// <param name="text">The text displayed on the button.</param>
        /// <param name="inset">The nine-slice inset.</param>
        /// <param name="uvInset">The texture coordinate inset.</param>
        /// <param name="colour">The button's initial colour. Defaults to white.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no default canvas is assigned.
        /// </exception>
        public Button(Vector4 bounds, string text = "", float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : this(UICanvas.Default ?? throw new InvalidOperationException("Default Canvas must not be null for this Constructor. Try Creating a new UICanvas or assigning a UICanvas instance to UICanvas.Default"), bounds, text, inset, uvInset, colour) { }

        /// <summary>
        /// Handles mouse button presses, raising <see cref="OnPointerDown"/> when the pointer is within the button.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>True if the button was pressed.</returns>
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

        /// <summary>
        /// Updates the button's hover state and raises the appropriate hover event when it changes.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>Whether the button remains pressed.</returns>
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

        /// <summary>
        /// Handles mouse button releases, raising <see cref="OnClick"/> and performing page navigation when applicable.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>True if a click was handled.</returns>
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

        /// <summary>
        /// Updates the button's hover transition and pressed-state appearance.
        /// </summary>
        /// <param name="deltaTime">The elapsed time since the previous update, in seconds.</param>
        /// <param name="mouse">The current mouse state.</param>
        /// <param name="keyboard">The current keyboard state.</param>
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

        /// <summary>
        /// Submits the button's background and text label to the renderer.
        /// </summary>
        /// <param name="renderer">The renderer receiving the button's data.</param>
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