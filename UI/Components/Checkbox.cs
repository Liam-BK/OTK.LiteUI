using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OTK.LiteUI.Core;

namespace OTK.LiteUI.UI.Components
{
    /// <summary>
    /// A checkbox component that toggles between checked and unchecked states when clicked.
    /// </summary>
    public class Checkbox : NineSlice
    {
        private bool _isPressed = false;

        /// <summary>
        /// The texture displayed when the checkbox is unchecked.
        /// </summary>
        [UIProperty]
        public string UncheckedTexture { get; set; } = "";

        /// <summary>
        /// The texture displayed when the checkbox is checked.
        /// </summary>
        [UIProperty]
        public string CheckedTexture { get; set; } = "";

        /// <summary>
        /// The colour applied when the checkbox is unchecked.
        /// </summary>
        [UIProperty]
        public Vector4 UncheckedColour { get; set; }

        /// <summary>
        /// The colour applied when the checkbox is checked.
        /// </summary>
        [UIProperty]
        public Vector4 CheckedColour { get; set; }

        /// <summary>
        /// Gets or sets whether the checkbox is checked.
        /// </summary>
        [UIProperty]
        public bool Checked
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the checkbox bounds.
        /// The width is adjusted to match the height, keeping the checkbox square.
        /// The right edge and vertical bounds are preserved.
        /// </summary>
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

        /// <summary>
        /// Occurs when the checkbox is clicked.
        /// </summary>
        public event Action<MouseButton>? OnClick;

        /// <summary>
        /// Initializes a checkbox on the specified canvas.
        /// </summary>
        /// <param name="canvas">The canvas to register the checkbox with.</param>
        /// <param name="bounds">The initial bounds of the checkbox.</param>
        /// <param name="inset">The nine-slice inset.</param>
        /// <param name="uvInset">The texture coordinate inset.</param>
        /// <param name="colour">The initial colour for both checked and unchecked states. Defaults to white.</param>
        public Checkbox(UICanvas canvas, Vector4 bounds, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(canvas, bounds, inset, uvInset, colour)
        {
            Canvas = canvas;
            UncheckedColour = colour ?? Vector4.One;
            CheckedColour = colour ?? Vector4.One;
            if (GetType() == typeof(Checkbox)) Identifier = Canvas.GetNextIdentifier(typeof(Checkbox));
        }

        /// <summary>
        /// Initializes a checkbox on the default canvas.
        /// </summary>
        /// <param name="bounds">The initial bounds of the checkbox.</param>
        /// <param name="inset">The nine-slice inset.</param>
        /// <param name="uvInset">The texture coordinate inset.</param>
        /// <param name="colour">The initial colour for both checked and unchecked states. Defaults to white.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no default canvas is assigned.
        /// </exception>
        public Checkbox(Vector4 bounds, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : this(UICanvas.Default ?? throw new InvalidOperationException("Default Canvas must not be null for this Constructor. Try Creating a new UICanvas or assigning a UICanvas instance to UICanvas.Default"), bounds, inset, uvInset, colour) { }

        /// <summary>
        /// Handles mouse button presses within the checkbox bounds.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>True if the checkbox was pressed.</returns>
        public override bool OnClickDown(MouseState mouse)
        {
            if (!IsVisible) return false;
            _isPressed = WithinBounds(mouse);

            return _isPressed;
        }

        /// <summary>
        /// Updates the pressed state when the pointer moves.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>Whether the checkbox remains pressed.</returns>
        public override bool OnMouseMove(MouseState mouse)
        {
            if (!IsVisible) return false;
            if (_isPressed)
            {
                _isPressed = WithinBounds(mouse);
            }
            return _isPressed;
        }

        /// <summary>
        /// Toggles the checked state when the checkbox is clicked.
        /// Raises <see cref="OnClick"/> when a mouse button is released within its bounds.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>True if the checkbox was clicked.</returns>
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

        /// <summary>
        /// Updates the checkbox texture and colour according to its checked and pressed states.
        /// </summary>
        /// <param name="deltaTime">The elapsed time since the previous update, in seconds.</param>
        /// <param name="mouse">The current mouse state.</param>
        /// <param name="keyboard">The current keyboard state.</param>
        public override void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
        {
            if (!IsVisible || Colour.W <= 0) return;
            var multiplier = new Vector4(_isPressed ? 0.5f : 1.0f, _isPressed ? 0.5f : 1.0f, _isPressed ? 0.5f : 1.0f, 1);
            Texture = Checked ? CheckedTexture : UncheckedTexture;
            Colour = (Checked ? CheckedColour : UncheckedColour) * multiplier;
        }
    }
}