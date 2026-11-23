using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Rendering;
using OpenTK.Windowing.Common.Input;

namespace OTK.LiteUI.UI.Components
{
    /// <summary>
    /// A container component that arranges child components using a layout.
    /// Supports scrolling, clipping, and optional resizing.
    /// </summary>
    public class Panel : NineSlice, IUIContainer, IScrollable
    {
        /// <summary>
        /// Gets the child components managed by this panel.
        /// </summary>
        private List<UIComponent> _children = new List<UIComponent>();

        /// <summary>
        /// Gets or sets the multiplier applied to mouse wheel scrolling.
        /// </summary>
        public static float ScrollMultiplier = 5.0f;

        /// <summary>
        /// Gets or sets the anchor used by the panel and its children.
        /// </summary>
        public override AnchorType Anchor
        {
            get => base.Anchor; set
            {
                base.Anchor = value;
                foreach (var child in Children)
                {
                    child.Anchor = value;
                }
            }
        }

        public List<UIComponent> Children => _children;

        /// <summary>
        /// Gets the maximum horizontal and vertical scroll offsets required
        /// to reveal the panel's entire content.
        /// </summary>
        public Vector2 MaxScroll
        {
            get
            {
                var view = ViewPort;
                var contentBounds = ContentBounds;
                var contentWidth = Math.Abs(contentBounds.Z - contentBounds.X);
                var contentHeight = Math.Abs(contentBounds.W - contentBounds.Y);
                return new Vector2(Math.Max(contentWidth - (view.Z - view.X), 0), Math.Max(contentHeight - (view.W - view.Y), 0));
            }
        }

        private Vector2 _scrollOffset = Vector2.Zero;

        /// <summary>
        /// Gets or sets the current horizontal and vertical scroll offsets.
        /// Values are clamped to the available scroll range.
        /// </summary>
        public Vector2 ScrollOffset
        {
            get
            {
                return _scrollOffset;
            }
            set
            {
                _scrollOffset = new Vector2(Math.Clamp(value.X, 0, MaxScroll.X), Math.Clamp(value.Y, 0, MaxScroll.Y));
            }
        }

        private Vector4 ContentBounds
        {
            get
            {
                if (Children.Count == 0)
                {
                    return Vector4.Zero;
                }
                var result = Children[0].Bounds;
                for (int i = 0; i < Children.Count; i++)
                {
                    result.X = Math.Min(Children[i].Bounds.X, result.X);
                    result.Y = Math.Min(Children[i].Bounds.Y, result.Y);
                    result.Z = Math.Max(Children[i].Bounds.Z, result.Z);
                    result.W = Math.Max(Children[i].Bounds.W, result.W);
                }
                return result;
            }
        }

        /// <summary>
        /// Gets or sets the bounds of the panel and updates the clipping bounds
        /// of its children.
        /// </summary>
        public override Vector4 Bounds
        {
            get => base.Bounds;
            set
            {
                base.Bounds = value;
                var view = ViewPort;
                foreach (var child in Children)
                {
                    child.ClipBounds = view;
                }
            }
        }

        /// <summary>
        /// Gets or sets the layout used to arrange the panel's children.
        /// </summary>
        [UIProperty]
        public ILayout Layout
        {
            private get;
            set;
        }

        /// <summary>
        /// Gets the visible content area inside the panel's inset.
        /// </summary>
        public Vector4 ViewPort => new(Bounds.X + Inset, Bounds.Y + Inset, Bounds.Z - Inset, Bounds.W - Inset);

        private bool _isResizable = false;

        /// <summary>
        /// Gets or sets whether the panel can be resized by dragging its edges.
        /// </summary>
        [UIProperty]
        public bool IsResizable
        {
            get
            {
                return _isResizable;
            }
            set
            {
                _isResizable = value;
            }
        }

        [Flags]
        protected enum ResizingFlags
        {
            None = 0,
            Left = 1,
            Bottom = 1 << 1,
            Right = 1 << 2,
            Top = 1 << 3
        }

        /// <summary>
        /// Gets or sets the resize edges currently being interacted with.
        /// </summary>
        public int ActiveResizeFlags = (int)ResizingFlags.None;

        /// <summary>
        /// Gets whether the panel is currently being resized.
        /// </summary>
        public bool isResizing = false;

        /// <summary>
        /// Gets or sets whether the panel and its children are visible.
        /// </summary>
        public override bool IsVisible
        {
            get => base.IsVisible;
            set
            {
                base.IsVisible = value;
                foreach (var element in Children)
                {
                    element.IsVisible = value;
                }
            }
        }

        /// <summary>
        /// Initializes a panel on the specified canvas using the supplied layout.
        /// </summary>
        /// <param name="canvas">The canvas to register the panel with.</param>
        /// <param name="bounds">The bounds of the panel.</param>
        /// <param name="layout">The layout used to arrange child components.</param>
        /// <param name="inset">The inset applied to the panel's background.</param>
        /// <param name="uvInset">The texture coordinate inset of the background.</param>
        /// <param name="colour">The background colour of the panel.</param>
        public Panel(UICanvas canvas, Vector4 bounds, ILayout layout, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(canvas, bounds, inset, uvInset, colour)
        {
            Canvas = canvas;
            Layout = layout;
            if (GetType() == typeof(Panel)) Identifier = Canvas.GetNextIdentifier(typeof(Panel));
        }

        /// <summary>
        /// Initializes a panel on the default canvas using the supplied layout.
        /// </summary>
        /// <param name="bounds">The bounds of the panel.</param>
        /// <param name="layout">The layout used to arrange child components.</param>
        /// <param name="inset">The inset applied to the panel's background.</param>
        /// <param name="uvInset">The texture coordinate inset of the background.</param>
        /// <param name="colour">The background colour of the panel.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no default canvas is assigned.
        /// </exception>
        public Panel(Vector4 bounds, ILayout layout, float inset = 10, float uvInset = 0.25f, Vector4? colour = null) : this(UICanvas.Default ?? throw new InvalidOperationException("Default Canvas must not be null for this Constructor. Try Creating a new UICanvas or assigning a UICanvas instance to UICanvas.Default"), bounds, layout, inset, uvInset, colour) { }

        private void ApplyLayout()
        {
            Layout.Apply(ViewPort, Children);
            foreach (var child in Children)
            {
                var bounds = child.Bounds;
                var view = ViewPort;
                bounds = new Vector4(bounds.X - ScrollOffset.X, bounds.Y + ScrollOffset.Y, bounds.Z - ScrollOffset.X, bounds.W + ScrollOffset.Y);
                child.Bounds = bounds;
            }
        }

        /// <summary>
        /// Adds a component to the panel, assigning it as a child and arranging
        /// the panel's contents. If null is supplied, a blank component is added instead.
        /// </summary>
        /// <param name="child">The component to add, or null to add a blank component.</param>
        public void AddChild(UIComponent? child)
        {
            if (child is null)
            {
                AddChild(new Blank(Canvas));
                return;
            }
            Canvas.Deregister(child);
            child.Anchor = Anchor;
            child.Parent = this;
            child.ClipBounds = ViewPort;
            Children.Add(child);
            ApplyLayout();
        }

        /// <summary>
        /// Replaces the layout used to arrange the panel's children.
        /// </summary>
        /// <param name="layout">The layout to apply.</param>
        public void SetLayout(ILayout layout)
        {
            Layout = layout;
        }

        /// <summary>
        /// Removes a component from the panel and rearranges the remaining children.
        /// </summary>
        /// <param name="child">The component to remove.</param>
        public void RemoveChild(UIComponent child)
        {
            Children.Remove(child);
            ApplyLayout();
        }

        /// <summary>
        /// Removes all child components from the panel.
        /// </summary>
        public void Clear()
        {
            Children.Clear();
        }

        private bool SetFlags(MouseState mouse)
        {
            if (Parent is null || Parent is not UIComponent && _isResizable && !isResizing)
            {
                var anchorOffset = Canvas.AnchorTypeOffsets[(int)Anchor](Canvas.CanvasSize, this);
                var convertedMouse = Canvas.ConvertMouseScreenCoords(mouse.Position) - anchorOffset;
                var maxEdgeOffset = 3;

                ActiveResizeFlags = (int)ResizingFlags.None;

                if (convertedMouse.X >= Bounds.X - maxEdgeOffset && convertedMouse.X <= Bounds.X + maxEdgeOffset && convertedMouse.Y >= Bounds.Y - maxEdgeOffset && convertedMouse.Y <= Bounds.W + maxEdgeOffset)
                {
                    ActiveResizeFlags = ActiveResizeFlags | (int)ResizingFlags.Left;
                }
                if (convertedMouse.Y >= Bounds.Y - maxEdgeOffset && convertedMouse.Y <= Bounds.Y + maxEdgeOffset && convertedMouse.X >= Bounds.X - maxEdgeOffset && convertedMouse.X <= Bounds.Z + maxEdgeOffset)
                {
                    ActiveResizeFlags = ActiveResizeFlags | (int)ResizingFlags.Bottom;
                }
                if (convertedMouse.X >= Bounds.Z - maxEdgeOffset && convertedMouse.X <= Bounds.Z + maxEdgeOffset && convertedMouse.Y >= Bounds.Y - maxEdgeOffset && convertedMouse.Y <= Bounds.W + maxEdgeOffset)
                {
                    ActiveResizeFlags = ActiveResizeFlags | (int)ResizingFlags.Right;
                }
                if (convertedMouse.Y >= Bounds.W - maxEdgeOffset && convertedMouse.Y <= Bounds.W + maxEdgeOffset && convertedMouse.X >= Bounds.X - maxEdgeOffset && convertedMouse.X <= Bounds.Z + maxEdgeOffset)
                {
                    ActiveResizeFlags = ActiveResizeFlags | (int)ResizingFlags.Top;
                }
                if (ActiveResizeFlags != (int)ResizingFlags.None)
                {
                    if (ActiveResizeFlags == (int)ResizingFlags.Left || ActiveResizeFlags == (int)ResizingFlags.Right)
                    {
                        Canvas.SetCursor(MouseCursor.ResizeEW);
                    }
                    else if (ActiveResizeFlags == (int)ResizingFlags.Bottom || ActiveResizeFlags == (int)ResizingFlags.Top)
                    {
                        Canvas.SetCursor(MouseCursor.ResizeNS);
                    }
                    else if (ActiveResizeFlags == ((int)ResizingFlags.Left | (int)ResizingFlags.Bottom) || ActiveResizeFlags == ((int)ResizingFlags.Right | (int)ResizingFlags.Top))
                    {
                        Canvas.SetCursor(MouseCursor.ResizeNESW);
                    }
                    else
                    {
                        Canvas.SetCursor(MouseCursor.ResizeNWSE);
                    }
                    return true;
                }
            }
            return false;
        }

        private bool UpdateBoundsForResize(MouseState mouse)
        {
            if (isResizing)
            {
                var anchorOffset = Canvas.AnchorTypeOffsets[(int)Anchor](Canvas.CanvasSize, this);
                var convertedMouse = Canvas.ConvertMouseScreenCoords(mouse.Position) - anchorOffset;
                if (ActiveResizeFlags == (int)ResizingFlags.Left)
                {
                    Bounds = new Vector4(convertedMouse.X, Bounds.Y, Bounds.Z, Bounds.W);
                }
                else if (ActiveResizeFlags == (int)ResizingFlags.Bottom)
                {
                    Bounds = new Vector4(Bounds.X, convertedMouse.Y, Bounds.Z, Bounds.W);
                }
                else if (ActiveResizeFlags == (int)ResizingFlags.Right)
                {
                    Bounds = new Vector4(Bounds.X, Bounds.Y, convertedMouse.X, Bounds.W);
                }
                else if (ActiveResizeFlags == (int)ResizingFlags.Top)
                {
                    Bounds = new Vector4(Bounds.X, Bounds.Y, Bounds.Z, convertedMouse.Y);
                }
                else if (ActiveResizeFlags == ((int)ResizingFlags.Left | (int)ResizingFlags.Bottom))
                {
                    Bounds = new Vector4(convertedMouse.X, convertedMouse.Y, Bounds.Z, Bounds.W);
                }
                else if (ActiveResizeFlags == ((int)ResizingFlags.Left | (int)ResizingFlags.Top))
                {
                    Bounds = new Vector4(convertedMouse.X, Bounds.Y, Bounds.Z, convertedMouse.Y);
                }
                else if (ActiveResizeFlags == ((int)ResizingFlags.Right | (int)ResizingFlags.Bottom))
                {
                    Bounds = new Vector4(Bounds.X, convertedMouse.Y, convertedMouse.X, Bounds.W);
                }
                else
                {
                    Bounds = new Vector4(Bounds.X, Bounds.Y, convertedMouse.X, convertedMouse.Y);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handles mouse button presses, forwarding input to child components
        /// and initiating panel resizing when applicable.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>
        /// <see langword="true"/> if the input was handled; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public override bool OnClickDown(MouseState mouse)
        {
            if (!IsVisible) return false;
            if (ActiveResizeFlags != (int)ResizingFlags.None)
            {
                isResizing = true;
                Canvas.LockCursorAppearance = true;
                return true;
            }
            if (!WithinBounds(mouse)) return false;
            foreach (var element in Children)
            {
                bool result = element.OnClickDown(mouse);
                if (result) return true;
            }
            return base.OnClickDown(mouse);
        }

        /// <summary>
        /// Forwards mouse button presses to child components without consuming the input.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        public override void OnClickDownNoConsume(MouseState mouse)
        {
            if (!IsVisible) return;
            foreach (var element in Children)
            {
                element.OnClickDownNoConsume(mouse);
            }
            base.OnClickDownNoConsume(mouse);
        }

        /// <summary>
        /// Handles mouse button releases and forwards them to child components.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>
        /// <see langword="true"/> if the input was handled; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public override bool OnClickUp(MouseState mouse)
        {
            if (!IsVisible || !WithinBounds(mouse)) return false;
            foreach (var element in Children)
            {
                bool result = element.OnClickUp(mouse);
                if (result) return true;
            }
            return base.OnClickUp(mouse);
        }

        /// <summary>
        /// Forwards mouse button releases to child components without consuming the input.
        /// Also ends any active panel resizing operation.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        public override void OnClickUpNoConsume(MouseState mouse)
        {
            if (!IsVisible) return;
            foreach (var element in Children)
            {
                element.OnClickUpNoConsume(mouse);
            }
            isResizing = false;
            Canvas.LockCursorAppearance = false;
            base.OnClickUpNoConsume(mouse);
        }

        /// <summary>
        /// Handles mouse movement, child input, and panel resizing.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>
        /// <see langword="true"/> if the input was consumed, otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public override bool OnMouseMove(MouseState mouse)
        {
            if (!IsVisible) return false;

            UpdateBoundsForResize(mouse);
            if (SetFlags(mouse)) return true;

            if (!WithinBounds(mouse)) return false;

            foreach (var element in Children)
            {
                bool result = element.OnMouseMove(mouse);
                if (result) return true;
            }
            return base.OnMouseMove(mouse);
        }

        /// <summary>
        /// Forwards mouse movement to child components without consuming the input.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        public override void OnMouseMoveNoConsume(MouseState mouse)
        {
            if (!IsVisible) return;
            foreach (var element in Children)
            {
                element.OnMouseMoveNoConsume(mouse);
            }
            base.OnMouseMoveNoConsume(mouse);
        }

        /// <summary>
        /// Handles mouse wheel input, forwarding it to children before scrolling the panel.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>
        /// The result of the base implementation.
        /// </returns>
        public override bool OnMouseWheel(MouseState mouse)
        {
            if (!IsVisible || !WithinBounds(mouse)) return false;
            foreach (var element in Children)
            {
                bool result = element.OnMouseWheel(mouse);
                if (result)
                {
                    ApplyLayout();
                    return true;
                }
            }
            ScrollOffset = new Vector2(ScrollOffset.X - mouse.ScrollDelta.X * ScrollMultiplier, ScrollOffset.Y - mouse.ScrollDelta.Y * ScrollMultiplier);
            ApplyLayout();
            return base.OnMouseWheel(mouse);
        }

        /// <summary>
        /// Forwards mouse wheel input to child components without consuming the input.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        public override void OnMouseWheelNoConsume(MouseState mouse)
        {
            if (!IsVisible) return;
            foreach (var element in Children)
            {
                element.OnMouseWheelNoConsume(mouse);
            }
            base.OnMouseWheelNoConsume(mouse);
        }

        /// <summary>
        /// Forwards keyboard key presses to child components.
        /// </summary>
        /// <param name="e">The keyboard event.</param>
        public override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (!IsVisible) return;
            foreach (var element in Children)
            {
                element.OnKeyDown(e);
            }
        }

        /// <summary>
        /// Forwards keyboard key releases to child components.
        /// </summary>
        /// <param name="e">The keyboard event.</param>
        public override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            if (!IsVisible) return;
            foreach (var element in Children)
            {
                element.OnKeyUp(e);
            }
        }

        /// <summary>
        /// Forwards text input to child components.
        /// </summary>
        /// <param name="e">The text input event.</param>
        public override void OnTextInput(TextInputEventArgs e)
        {
            if (!IsVisible) return;
            foreach (var element in Children)
            {
                element.OnTextInput(e);
            }
        }

        /// <summary>
        /// Applies the panel layout and updates visible child components.
        /// </summary>
        /// <param name="deltaTime">The elapsed time since the previous update.</param>
        /// <param name="mouse">The current mouse state.</param>
        /// <param name="keyboard">The current keyboard state.</param>
        public override void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
        {
            if (!IsVisible) return;
            if (Layout is not null) ApplyLayout();
            foreach (var element in Children)
            {
                if (element.Bounds.X > Bounds.Z || element.Bounds.Z < Bounds.X || element.Bounds.Y > Bounds.W || element.Bounds.W < Bounds.Y) continue;
                element.OnUpdate(deltaTime, mouse, keyboard);
            }
        }

        /// <summary>
        /// Submits the panel background and visible renderable children to the renderer.
        /// </summary>
        /// <param name="renderer">The renderer receiving the panel data.</param>
        public override void SubmitData(Renderer renderer)
        {
            if (!IsVisible) return;
            if (Colour.W > 0) base.SubmitData(renderer);
            foreach (var element in Children)
            {
                if (element.Bounds.X > Bounds.Z || element.Bounds.Z < Bounds.X || element.Bounds.Y > Bounds.W || element.Bounds.W < Bounds.Y) continue;
                if (element is IRenderable renderable)
                {
                    renderable.SubmitData(renderer);
                }
            }
        }
    }
}