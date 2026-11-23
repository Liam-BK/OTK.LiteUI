using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.UI.Rendering;

namespace OTK.LiteUI.UI.Components
{
    /// <summary>
    /// A scrollbar that supports horizontal and vertical scrolling, with an adjustable thumb
    /// whose size reflects the visible portion of the content.
    /// Can be linked to an <see cref="IScrollable"/> component to synchronize its scroll offset.
    /// </summary>
    public class ScrollBar : NineSlice
    {
        /// <summary>
        /// Gets or sets the anchor used by both the scrollbar and its thumb.
        /// </summary>
        [UIProperty]
        public override AnchorType Anchor
        {
            get => base.Anchor; set
            {
                base.Anchor = value;
                _thumb.Anchor = value;
            }
        }

        /// <summary>
        /// Gets or sets the scrollbar's orientation.
        /// Automatically determined from the bounds when they are assigned.
        /// </summary>
        [UIProperty]
        public ComponentOrientation Orientation { get; set; }

        private NineSlice _thumb;

        private Vector2 _contentSize = Vector2.One;

        /// <summary>
        /// Gets or sets the total size of the scrollable content.
        /// Used with <see cref="ViewSize"/> to determine the thumb size.
        /// </summary>
        [UIProperty]
        public Vector2 ContentSize
        {
            get
            {
                return _contentSize;
            }
            set
            {
                _contentSize = value;
                UpdateThumbFromValue();
            }
        }

        private Vector2 _viewSize = Vector2.One;

        /// <summary>
        /// Gets or sets the size of the visible portion of the content.
        /// Used with <see cref="ContentSize"/> to determine the thumb size.
        /// </summary>
        [UIProperty]
        public Vector2 ViewSize
        {
            get
            {
                return _viewSize;
            }
            set
            {
                _viewSize = value;
                UpdateThumbFromValue();
            }
        }

        private float _value = 0.0f;

        /// <summary>
        /// Gets or sets the normalized scroll position, clamped between 0 and 1.
        /// </summary>
        [UIProperty]
        public float Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = Math.Clamp(value, 0, 1);
                UpdateThumbFromValue();
            }
        }

        /// <summary>
        /// Sets the texture used by the scrollbar thumb.
        /// </summary>
        [UIProperty]
        public string ThumbTexture
        {
            set
            {
                _thumb.Texture = value;
            }
        }

        private Vector4 _thumbColour;

        /// <summary>
        /// Gets or sets the colour applied to the scrollbar thumb.
        /// </summary>
        [UIProperty]
        public Vector4 ThumbColour
        {
            get
            {
                return _thumbColour;
            }
            set
            {
                _thumbColour = value;
            }
        }

        /// <summary>
        /// Gets or sets the scrollbar bounds.
        /// The orientation is determined by whether the bounds are wider or taller.
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
                Orientation = Width >= Height ? ComponentOrientation.Horizontal : ComponentOrientation.Vertical;
                if (_thumb is not null) UpdateThumbFromValue();
            }
        }

        /// <summary>
        /// The sensitivity multiplier applied to mouse wheel scrolling.
        /// </summary>
        public const float scrollSensitivity = 0.4f;

        private bool _isPressed = false;

        private Vector2 _clickOffset = Vector2.Zero;

        /// <summary>
        /// Gets or sets whether the scrollbar and its thumb are visible.
        /// </summary>
        [UIProperty]
        public override bool IsVisible
        {
            get => base.IsVisible;
            set
            {
                base.IsVisible = value;
                _thumb.IsVisible = value;
            }
        }

        private static bool HorizontalScroll = false;

        private IScrollable? _scrollingComponent;

        /// <summary>
        /// Gets or sets the component whose scroll state is synchronized with this scrollbar.
        /// Assigning a component immediately synchronizes the scrollbar with its current state.
        /// </summary>
        public IScrollable? ScrollingComponent
        {
            get => _scrollingComponent;
            set
            {
                _scrollingComponent = value;
                if (_scrollingComponent is null) return;
                SyncFromScrollable();
            }
        }

        /// <summary>
        /// Initializes a scrollbar on the specified canvas.
        /// </summary>
        /// <param name="canvas">The canvas to register the scrollbar with.</param>
        /// <param name="bounds">The bounds of the scrollbar.</param>
        /// <param name="inset">The corner inset of the scrollbar background.</param>
        /// <param name="uvInset">The texture coordinate inset of the scrollbar background.</param>
        /// <param name="colour">The base colour of the scrollbar background.</param>
        public ScrollBar(UICanvas canvas, Vector4 bounds, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(canvas, bounds, inset, uvInset, colour)
        {
            Canvas = canvas;
            Orientation = Width >= Height ? ComponentOrientation.Horizontal : ComponentOrientation.Vertical;
            _thumb = new NineSlice(Canvas, bounds);
            _thumbColour = colour ?? Vector4.One;
            Canvas.Deregister(_thumb);
            if (GetType() == typeof(ScrollBar)) Identifier = Canvas.GetNextIdentifier(typeof(ScrollBar));
        }

        /// <summary>
        /// Initializes a scrollbar on the default canvas.
        /// </summary>
        /// <param name="bounds">The bounds of the scrollbar.</param>
        /// <param name="inset">The corner inset of the scrollbar background.</param>
        /// <param name="uvInset">The texture coordinate inset of the scrollbar background.</param>
        /// <param name="colour">The base colour of the scrollbar background.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no default canvas is assigned.
        /// </exception>
        public ScrollBar(Vector4 bounds, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : this(UICanvas.Default ?? throw new InvalidOperationException("Default Canvas must not be null for this Constructor. Try Creating a new UICanvas or assigning a UICanvas instance to UICanvas.Default"), bounds, inset, uvInset, colour) { }

        private void SyncFromScrollable()
        {
            if (ScrollingComponent is null)
                return;

            Vector4 viewport = ScrollingComponent.ViewPort;
            ViewSize = new Vector2(viewport.Z - viewport.X, viewport.W - viewport.Y);
            Vector2 maxScroll = ScrollingComponent.MaxScroll;
            ContentSize = ViewSize + maxScroll;
            float maxOffset = Orientation == ComponentOrientation.Horizontal ? maxScroll.X : maxScroll.Y;
            float currentOffset = Orientation == ComponentOrientation.Horizontal ? ScrollingComponent.ScrollOffset.X : ScrollingComponent.ScrollOffset.Y;
            Value = maxOffset > 0 ? currentOffset / maxOffset : 0;
        }

        private void Refresh()
        {
            if (ScrollingComponent is null) return;

            SyncFromScrollable();
            UpdateThumbFromValue();
            SyncToScrollable();
        }

        private void SyncToScrollable()
        {
            if (ScrollingComponent is null)
                return;

            Vector2 maxScroll = ScrollingComponent.MaxScroll;
            Vector2 offset = ScrollingComponent.ScrollOffset;

            if (Orientation == ComponentOrientation.Horizontal)
                offset.X = Value * maxScroll.X;
            else
                offset.Y = Value * maxScroll.Y;

            ScrollingComponent.ScrollOffset = offset;
        }

        private void UpdateThumbFromValue()
        {
            if (_contentSize.X == 0 || _contentSize.Y == 0) return;
            Vector2 proportion = new Vector2(Math.Min(_viewSize.X / _contentSize.X, 1.0f), Math.Min(_viewSize.Y / _contentSize.Y, 1.0f));
            float thumbWidth = Orientation == ComponentOrientation.Horizontal ? Width * proportion.X : Width;
            float thumbHeight = Orientation == ComponentOrientation.Vertical ? Height * proportion.Y : Height;
            float maxOffset = 1 - (Orientation == ComponentOrientation.Horizontal ? proportion.X : proportion.Y);
            float left = Orientation == ComponentOrientation.Horizontal ? Bounds.X + maxOffset * _value * Width : Bounds.X;
            float right = Orientation == ComponentOrientation.Horizontal ? left + thumbWidth : Bounds.Z;
            float top = Orientation == ComponentOrientation.Vertical ? Bounds.W - maxOffset * _value * Height : Bounds.W;
            float bottom = Orientation == ComponentOrientation.Vertical ? top - thumbHeight : Bounds.Y;
            _thumb.Bounds = new Vector4(left, bottom, right, top);
        }

        private void UpdateValueFromThumb()
        {
            if (Orientation == ComponentOrientation.Horizontal)
            {
                var denominator = Math.Max(Width - _thumb.Width, 0.0f);
                _value = Math.Abs(denominator) < 0.0005f ? 0 : (_thumb.Bounds.X - Bounds.X) / denominator;
            }
            else
            {
                var denominator = Math.Max(Height - _thumb.Height, 0.0f);
                _value = Math.Abs(denominator) < 0.0005f ? 0 : (Bounds.W - _thumb.Bounds.W) / denominator;
            }
            _value = Math.Clamp(_value, 0.0f, 1.0f);
            SyncToScrollable();
        }

        private void SetThumbPos(float x, float y)
        {
            Vector2 halfThumbSize = new Vector2(_thumb.Width, _thumb.Height) * 0.5f;
            _thumb.Bounds = new Vector4(x - halfThumbSize.X, y - halfThumbSize.Y, x + halfThumbSize.X, y + halfThumbSize.Y);
        }

        /// <summary>
        /// Handles mouse button presses, allowing the thumb to be dragged or the track
        /// to be clicked to change the scroll position.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>Whether the scrollbar handled the press.</returns>
        public override bool OnClickDown(MouseState mouse)
        {
            if (!IsVisible) return false;
            var convertedMouse = Canvas.ConvertMouseScreenCoords(mouse.Position);
            var visualOffset = Canvas.AnchorTypeOffsets[(int)Anchor](Canvas.CanvasSize, this);
            convertedMouse -= visualOffset;
            if (_thumb.WithinBounds(mouse))
            {
                _isPressed = true;
                _clickOffset = convertedMouse - _thumb.Center;
            }
            else if (WithinBounds(mouse))
            {
                if (Orientation == ComponentOrientation.Horizontal)
                {
                    float difference = convertedMouse.X - _thumb.Center.X;
                    float halfSize = _thumb.Width * 0.5f * (difference > 0 ? 1 : -1);
                    SetThumbPos(_thumb.Center.X + difference - halfSize, _thumb.Center.Y);
                }
                else
                {
                    float difference = convertedMouse.Y - _thumb.Center.Y;
                    float halfSize = _thumb.Height * 0.5f * (difference > 0 ? 1 : -1);
                    SetThumbPos(_thumb.Center.X, _thumb.Center.Y + difference - halfSize);
                }
                _isPressed = true;
                _clickOffset = convertedMouse - _thumb.Center;
                UpdateValueFromThumb();
            }
            return _isPressed;
        }

        /// <summary>
        /// Updates the thumb position while it is being dragged.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>Whether the scrollbar is handling the mouse movement.</returns>
        public override bool OnMouseMove(MouseState mouse)
        {
            if (_isPressed)
            {
                var convertedMouse = Canvas.ConvertMouseScreenCoords(mouse.Position);
                var visualOffset = Canvas.AnchorTypeOffsets[(int)Anchor](Canvas.CanvasSize, this);
                convertedMouse -= visualOffset;
                if (Orientation == ComponentOrientation.Horizontal)
                {
                    SetThumbPos(Math.Clamp(convertedMouse.X - _clickOffset.X, Bounds.X + _thumb.Width * 0.5f, Bounds.Z - _thumb.Width * 0.5f), Center.Y);
                }
                if (Orientation == ComponentOrientation.Vertical)
                {
                    SetThumbPos(Center.X, Math.Clamp(convertedMouse.Y - _clickOffset.Y, Bounds.Y + _thumb.Height * 0.5f, Bounds.W - _thumb.Height * 0.5f));
                }
                UpdateValueFromThumb();
                return true;
            }
            return base.OnMouseMove(mouse);
        }

        /// <summary>
        /// Updates the thumb position while dragging without consuming the mouse movement, regardless of if it hits something.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        public override void OnMouseMoveNoConsume(MouseState mouse)
        {
            if (_isPressed)
            {
                var convertedMouse = Canvas.ConvertMouseScreenCoords(mouse.Position);
                var visualOffset = Canvas.AnchorTypeOffsets[(int)Anchor](Canvas.CanvasSize, this);
                convertedMouse -= visualOffset;
                if (Orientation == ComponentOrientation.Horizontal)
                {
                    SetThumbPos(Math.Clamp(convertedMouse.X - _clickOffset.X, Bounds.X + _thumb.Width * 0.5f, Bounds.Z - _thumb.Width * 0.5f), Center.Y);
                }
                if (Orientation == ComponentOrientation.Vertical)
                {
                    SetThumbPos(Center.X, Math.Clamp(convertedMouse.Y - _clickOffset.Y, Bounds.Y + _thumb.Height * 0.5f, Bounds.W - _thumb.Height * 0.5f));
                }
                UpdateValueFromThumb();
                return;
            }
        }

        /// <summary>
        /// Ends thumb dragging and resets the drag offset.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>The result of the base click-release handler.</returns>
        public override bool OnClickUp(MouseState mouse)
        {
            _isPressed = false;
            _clickOffset = Vector2.Zero;
            return base.OnClickUp(mouse);
        }

        /// <summary>
        /// Ends thumb dragging without consuming the click release, regardless of if it hits something.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        public override void OnClickUpNoConsume(MouseState mouse)
        {
            _isPressed = false;
            _clickOffset = Vector2.Zero;
            base.OnClickUpNoConsume(mouse);
        }

        /// <summary>
        /// Changes the scroll position using the mouse wheel when the pointer is over the scrollbar.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>Whether the scrollbar handled the wheel input.</returns>
        public override bool OnMouseWheel(MouseState mouse)
        {
            if (!IsVisible) return false;
            if (WithinBounds(mouse))
            {
                if (Orientation == ComponentOrientation.Horizontal)
                {
                    float velocity = mouse.ScrollDelta.X * scrollSensitivity;
                    SetThumbPos(Math.Clamp(_thumb.Center.X - velocity, Bounds.X + _thumb.Width * 0.5f, Bounds.Z - _thumb.Width * 0.5f), Center.Y);
                    UpdateValueFromThumb();
                }
                else
                {
                    float velocity = mouse.ScrollDelta.Y * scrollSensitivity;
                    SetThumbPos(Center.X, Math.Clamp(_thumb.Center.Y + velocity, Bounds.Y + _thumb.Height * 0.5f, Bounds.W - _thumb.Height * 0.5f));
                    UpdateValueFromThumb();
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Synchronizes the scrollbar with its associated component when the pointer is
        /// over that component, without consuming the mouse wheel input.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        public override void OnMouseWheelNoConsume(MouseState mouse)
        {
            if (!IsVisible) return;
            if (ScrollingComponent is not null && ScrollingComponent is UIComponent component && component.WithinBounds(mouse))
            {
                SyncFromScrollable();
            }
            base.OnMouseWheelNoConsume(mouse);
        }

        /// <summary>
        /// Updates the thumb appearance, synchronizes the scroll position, and handles
        /// the release of the left mouse button.
        /// </summary>
        /// <param name="deltaTime">The elapsed time since the previous update.</param>
        /// <param name="mouse">The current mouse state.</param>
        /// <param name="keyboard">The current keyboard state.</param>
        public override void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
        {
            _thumb.Colour = _thumbColour * (_isPressed ? 0.5f : 1.0f);
            HorizontalScroll = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
            Refresh();
            if (mouse.IsButtonReleased(MouseButton.Left))
            {
                _isPressed = false;
                _clickOffset = Vector2.Zero;
            }
        }

        /// <summary>
        /// Submits the scrollbar background and thumb render data to the renderer.
        /// </summary>
        /// <param name="renderer">The renderer receiving the scrollbar data.</param>
        public override void SubmitData(Renderer renderer)
        {
            if (!IsVisible || Colour.W <= 0) return;
            base.SubmitData(renderer);
            _thumb.ClipBounds = ClipBounds;
            _thumb.SubmitData(renderer);
        }
    }
}