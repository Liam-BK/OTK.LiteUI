using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.Core;

/// <summary>
/// Specifies whether a component with a track is oriented horizontally or vertically.
/// </summary>
public enum ComponentOrientation
{
    /// <summary>
    /// Treats the component's track as horizontal.
    /// </summary>
    Horizontal,
    /// <summary>
    /// Treats the component's tracks as vertical.
    /// </summary>
    Vertical
}

/// <summary>
/// Specifies the position of a UI component relative to the canvas's associated GameWindow.
/// </summary>
public enum AnchorType
{
    /// <summary>Anchors to the bottom-left corner.</summary>
    BottomLeft = 0,
    /// <summary>Anchors to the bottom edge, centered horizontally.</summary>
    BottomCenter = 1,
    /// <summary>Anchors to the bottom-right corner.</summary>
    BottomRight = 2,
    /// <summary>Anchors to the left edge, centered vertically.</summary>
    MiddleLeft = 3,
    /// <summary>Anchors to the center of the canvas.</summary>
    Center = 4,
    /// <summary>Anchors to the right edge, centered vertically.</summary>
    MiddleRight = 5,
    /// <summary>Anchors to the top-left corner.</summary>
    TopLeft = 6,
    /// <summary>Anchors to the top edge, centered horizontally.</summary>
    TopCenter = 7,
    /// <summary>Anchors to the top-right corner.</summary>
    TopRight = 8
}

namespace OTK.LiteUI.UI.Components
{
    public abstract class UIComponent : UIElement
    {
        /// <summary>
        /// Provides the base functionality for UI components, including bounds,
        /// anchoring, visibility, clipping, colour, and input handling.
        /// </summary>
        [UIProperty]
        public virtual AnchorType Anchor { get; set; } = AnchorType.BottomLeft;

        private IUIContainer? _parent = null;

        /// <summary>
        /// Gets or sets the container that owns this component.
        /// </summary>
        public virtual IUIContainer? Parent { get => _parent; set => _parent = value; }

        /// <summary>
        /// Gets whether the component is visible, accounting for the visibility
        /// of its parent when the parent is a UI element.
        /// </summary>
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

        /// <summary>
        /// Gets the highest-level page containing this component, or null if it
        /// is not contained within a page.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the clipping bounds applied to the component.
        /// When the parent is another UI component, its clipping bounds are
        /// intersected with the component's own clipping bounds.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the component's bounds in canvas coordinates.
        /// </summary>
        [UIProperty]
        public virtual Vector4 Bounds
        {
            get
            {
                return _bounds;
            }
            set => _bounds = value;
        }

        /// <summary>
        /// Gets the center position of the component's bounds.
        /// </summary>
        public Vector2 Center => new Vector2((Bounds.X + Bounds.Z) * 0.5f, (Bounds.Y + Bounds.W) * 0.5f);

        /// <summary>
        /// Gets the height of the component's bounds.
        /// </summary>
        public float Height => Bounds.W - Bounds.Y;

        /// <summary>
        /// Gets the width of the component's bounds.
        /// </summary>
        public float Width => Bounds.Z - Bounds.X;

        protected Vector4 _colour = Vector4.One;

        /// <summary>
        /// Gets or sets the component's colour. Each channel is clamped to the range [0, 1].
        /// </summary>
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

        /// <summary>
        /// Gets whether the component can receive keyboard focus.
        /// </summary>
        public virtual bool CanFocus => false;

        /// <summary>
        /// Gets whether this component currently has keyboard focus.
        /// </summary>
        public bool IsFocused => CanFocus && UICanvas.FocusedComponent == this;

        /// <summary>
        /// Called when the component receives keyboard focus.
        /// Override to perform focus-related setup.
        /// </summary>
        public virtual void OnFocusGained() { }

        /// <summary>
        /// Called when the component loses keyboard focus.
        /// Override to perform focus-related cleanup.
        /// </summary>
        public virtual void OnFocusLost() { }

        /// <summary>
        /// Determines whether the mouse pointer is within the component's bounds.
        /// Converts the mouse position from screen coordinates to canvas coordinates.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>
        /// <see langword="true"/> if the pointer is within the component's bounds;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public virtual bool WithinBounds(MouseState mouse)
        {
            return WithinBounds(Canvas.ConvertMouseScreenCoords(mouse.Position));
        }

        /// <summary>
        /// Determines whether the specified canvas position is within the component's bounds,
        /// accounting for its anchor offset.
        /// </summary>
        /// <param name="position">The position to test, in canvas coordinates.</param>
        /// <returns>
        /// <see langword="true"/> if the position is within the component's bounds;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public bool WithinBounds(Vector2 position)
        {
            var anchorPos = Canvas.AnchorTypeOffsets[(int)Anchor](Canvas.CanvasSize, this);
            return position.X > Bounds.X + anchorPos.X && position.X <= Bounds.Z + anchorPos.X && position.Y > Bounds.Y + anchorPos.Y && position.Y <= Bounds.W + anchorPos.Y;
        }
    }
}