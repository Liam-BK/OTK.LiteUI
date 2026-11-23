using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Rendering;

namespace OTK.LiteUI.UI.Components
{
    /// <summary>
    /// A slider that allows a normalized value to be adjusted by dragging its thumb
    /// or clicking along its track.
    /// Supports horizontal and vertical orientations.
    /// </summary>
    public class Slider : NineSlice
    {
        /// <summary>
        /// Gets or sets whether the slider and its thumb are visible.
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

        /// <summary>
        /// Gets or sets the anchor position used by the slider and its thumb.
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
        /// Gets or sets the orientation of the slider.
        /// Automatically determined from the bounds when they are assigned.
        /// </summary>
        [UIProperty]
        public ComponentOrientation Orientation { get; set; }

        private NineSlice _thumb;

        private Vector2 _clickOffset = Vector2.Zero;

        private Vector4 _thumbColour;

        /// <summary>
        /// Gets or sets the colour applied to the slider thumb.
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

        private bool _thumbPressed = false;

        /// <summary>
        /// Sets the texture used by the slider thumb.
        /// </summary>
        [UIProperty]
        public string ThumbTexture
        {
            set
            {
                _thumb.Texture = value;
            }
        }

        private float _value = 0.0f;

        private float HalfThumbSize => Orientation == ComponentOrientation.Horizontal ? Height * 0.5f : Width * 0.5f;

        /// <summary>
        /// Gets or sets the normalized slider value, clamped between 0 and 1.
        /// Changing this value updates the thumb position and raises
        /// <see cref="OnValueChanged"/>.
        /// </summary>
        [UIProperty]
        public float Value
        {
            get => _value;
            set
            {
                bool valueChanged = _value != value;
                _value = Math.Clamp(value, 0, 1);
                SetThumbPositionFromValue();
                if (valueChanged) OnValueChanged?.Invoke(_value);
            }
        }

        /// <summary>
        /// Gets or sets the slider bounds.
        /// The orientation is determined by whether the bounds are wider or taller.
        /// Updating the bounds also repositions the thumb.
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
                if (_thumb is not null) SetThumbPositionFromValue();
            }
        }

        /// <summary>
        /// Occurs when the slider value changes, either through direct assignment
        /// or user interaction.
        /// </summary>
        public event Action<float>? OnValueChanged;

        /// <summary>
        /// Initializes a slider on the specified canvas.
        /// </summary>
        /// <param name="canvas">The canvas to register the slider with.</param>
        /// <param name="bounds">The bounds of the slider.</param>
        /// <param name="inset">The corner inset of the slider background.</param>
        /// <param name="uvInset">The texture coordinate inset of the slider background.</param>
        /// <param name="colour">The initial colour of the slider background and thumb.</param>
        public Slider(UICanvas canvas, Vector4 bounds, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(canvas, bounds, inset, uvInset, colour)
        {
            this.Canvas = canvas;
            Orientation = Width >= Height ? ComponentOrientation.Horizontal : ComponentOrientation.Vertical;
            float halfThumbSize = HalfThumbSize;
            float x = (Orientation == ComponentOrientation.Horizontal) ? Bounds.X + halfThumbSize : Bounds.X + Width * 0.5f;
            float y = (Orientation == ComponentOrientation.Horizontal) ? Bounds.Y + Height * 0.5f : Bounds.Y + halfThumbSize;
            _thumb = new NineSlice(canvas, new Vector4(x - halfThumbSize, y - halfThumbSize, x + halfThumbSize, y + halfThumbSize));
            _thumbColour = colour ?? Vector4.One;
            canvas.Deregister(_thumb);
            if (GetType() == typeof(Slider)) Identifier = Canvas.GetNextIdentifier(typeof(Slider));
        }

        /// <summary>
        /// Initializes a slider on the default canvas.
        /// </summary>
        /// <param name="bounds">The bounds of the slider.</param>
        /// <param name="inset">The corner inset of the slider background.</param>
        /// <param name="uvInset">The texture coordinate inset of the slider background.</param>
        /// <param name="colour">The initial colour of the slider background and thumb.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no default canvas is assigned.
        /// </exception>
        public Slider(Vector4 bounds, float inset = 10, float uvInset = 0.25f, Vector4? colour = null) : this(UICanvas.Default ?? throw new InvalidOperationException("Default Canvas must not be null for this Constructor. Try Creating a new UICanvas or assigning a UICanvas instance to UICanvas.Default"), bounds, inset, uvInset, colour) { }

        private void SetValueFromThumbPosition()
        {
            var halfThumbSize = HalfThumbSize;
            if (Orientation == ComponentOrientation.Horizontal)
            {
                _value = (_thumb.Center.X - (Bounds.X + halfThumbSize)) / Math.Max(1, Width - halfThumbSize * 2);
                _value = Math.Clamp(_value, 0, 1);
                OnValueChanged?.Invoke(_value);
                return;
            }
            _value = (_thumb.Center.Y - (Bounds.Y + halfThumbSize)) / Math.Max(1, Height - halfThumbSize * 2);
            _value = Math.Clamp(_value, 0, 1);
            OnValueChanged?.Invoke(_value);
        }

        private void SetThumbPositionFromValue()
        {
            var halfThumbSize = HalfThumbSize;
            float x = (Orientation == ComponentOrientation.Horizontal) ? MathHelper.Lerp(Bounds.X + halfThumbSize, Bounds.Z - halfThumbSize, _value) : Bounds.X + Width * 0.5f;
            float y = (Orientation == ComponentOrientation.Horizontal) ? Bounds.Y + Height * 0.5f : MathHelper.Lerp(Bounds.Y + halfThumbSize, Bounds.W - halfThumbSize, _value);
            SetThumbPos(x, y);
        }

        private void SetThumbPos(float x, float y)
        {
            var halfThumbSize = HalfThumbSize;
            _thumb.Bounds = new Vector4(x - halfThumbSize, y - halfThumbSize, x + halfThumbSize, y + halfThumbSize);
        }

        /// <summary>
        /// Determines whether the specified mouse position intersects the slider track
        /// or its thumb.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>Whether the mouse position is within the slider or its thumb.</returns>
        public override bool WithinBounds(MouseState mouse)
        {
            return base.WithinBounds(mouse) || _thumb.WithinBounds(mouse);
        }

        /// <summary>
        /// Handles mouse presses on the slider, allowing the thumb to be dragged
        /// or repositioned by clicking the track.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>Whether the slider handled the press.</returns>
        public override bool OnClickDown(MouseState mouse)
        {
            if (!IsVisible) return false;
            var convertedMouse = Canvas.ConvertMouseScreenCoords(mouse.Position);
            var visualOffset = Canvas.AnchorTypeOffsets[(int)Anchor](Canvas.CanvasSize, this);
            convertedMouse -= visualOffset;
            if (_thumb.WithinBounds(mouse))
            {
                _thumbPressed = true;
                _clickOffset = convertedMouse - _thumb.Center;
                return true;
            }
            else if (!_thumb.WithinBounds(mouse) && WithinBounds(mouse))
            {
                float halfThumbSize = Orientation == ComponentOrientation.Horizontal ? Height * 0.55f : Width * 0.55f;
                if (Orientation == ComponentOrientation.Horizontal)
                {
                    var x = convertedMouse.X;
                    var y = Center.Y;
                    x += ((x < _thumb.Center.X) ? 1 : -1) * halfThumbSize;
                    SetThumbPos(x, y);
                }
                else
                {
                    var x = Center.X;
                    var y = convertedMouse.Y;
                    y += ((y < _thumb.Center.Y) ? 1 : -1) * halfThumbSize;
                    SetThumbPos(x, y);
                }
                _clickOffset = convertedMouse - _thumb.Center;
                _thumbPressed = true;
                SetValueFromThumbPosition();
            }
            return base.OnClickDown(mouse);
        }

        /// <summary>
        /// Updates the thumb position and value while it is being dragged.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>Whether the slider handled the mouse movement.</returns>
        public override bool OnMouseMove(MouseState mouse)
        {
            if (!IsVisible) return false;
            var convertedMouse = Canvas.ConvertMouseScreenCoords(mouse.Position);
            var visualOffset = Canvas.AnchorTypeOffsets[(int)Anchor](Canvas.CanvasSize, this);
            convertedMouse -= visualOffset;
            if (_thumbPressed)
            {
                var halfThumbSize = HalfThumbSize;
                if (Orientation == ComponentOrientation.Horizontal)
                {
                    float x = Math.Clamp(convertedMouse.X - _clickOffset.X, Bounds.X + halfThumbSize, Bounds.Z - halfThumbSize);
                    float y = Center.Y;
                    SetThumbPos(x, y);
                    SetValueFromThumbPosition();
                }
                else
                {
                    float x = Center.X;
                    float y = Math.Clamp(convertedMouse.Y - _clickOffset.Y, Bounds.Y + halfThumbSize, Bounds.W - halfThumbSize);
                    SetThumbPos(x, y);
                    SetValueFromThumbPosition();
                }
                return true;
            }
            return base.OnMouseMove(mouse);
        }

        /// <summary>
        /// Ends thumb dragging and resets the drag offset.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>The result of the base click-release handler.</returns>
        public override bool OnClickUp(MouseState mouse)
        {
            if (!IsVisible) return false;
            _thumbPressed = false;
            _clickOffset = Vector2.Zero;
            return base.OnClickUp(mouse);
        }

        /// <summary>
        /// Updates the background and thumb appearance while the slider is visible.
        /// </summary>
        /// <param name="deltaTime">The elapsed time since the previous update.</param>
        /// <param name="mouse">The current mouse state.</param>
        /// <param name="keyboard">The current keyboard state.</param>
        public override void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
        {
            if (!IsVisible) return;
            base.OnUpdate(deltaTime, mouse, keyboard);
            _thumb.Colour = _thumbPressed ? _thumbColour * new Vector4(0.5f, 0.5f, 0.5f, 1.0f) : _thumbColour;
        }

        /// <summary>
        /// Submits the slider background and thumb to the renderer.
        /// </summary>
        /// <param name="renderer">The renderer receiving the slider data.</param>
        public override void SubmitData(Renderer renderer)
        {
            if (!IsVisible || Colour.W <= 0) return;
            base.SubmitData(renderer);
            _thumb.ClipBounds = ClipBounds;
            _thumb.SubmitData(renderer);
        }
    }
}