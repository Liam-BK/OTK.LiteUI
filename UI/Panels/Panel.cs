using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Rendering;
using OpenTK.Windowing.Common.Input;
using System.Reflection.Metadata.Ecma335;

namespace OTK.LiteUI.UI.Components
{
    public class Panel : NineSlice, IUIContainer, IScrollable
    {
        private List<UIComponent> _children = new List<UIComponent>();

        public static float ScrollMultiplier = 5.0f;

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

        public ILayout Layout
        {
            private get;
            set;
        }

        public Vector4 ViewPort => new(Bounds.X + Inset, Bounds.Y + Inset, Bounds.Z - Inset, Bounds.W - Inset);

        private bool _isResizable = false;

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

        public int ActiveResizeFlags = (int)ResizingFlags.None;

        public bool isResizing = false;

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

        public Panel(UICanvas canvas, Vector4 bounds, ILayout layout, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(canvas, bounds, inset, uvInset, colour)
        {
            Canvas = canvas;
            Layout = layout;
            if (GetType() == typeof(Panel)) Identifier = Canvas.GetNextIdentifier(typeof(Panel));
        }

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

        public void SetLayout(ILayout layout)
        {
            Layout = layout;
        }

        public void RemoveChild(UIComponent child)
        {
            Children.Remove(child);
            ApplyLayout();
        }

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

        public override void OnClickDownNoConsume(MouseState mouse)
        {
            if (!IsVisible) return;
            foreach (var element in Children)
            {
                element.OnClickDownNoConsume(mouse);
            }
            base.OnClickDownNoConsume(mouse);
        }

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

        public override void OnMouseMoveNoConsume(MouseState mouse)
        {
            if (!IsVisible) return;
            foreach (var element in Children)
            {
                element.OnMouseMoveNoConsume(mouse);
            }
            base.OnMouseMoveNoConsume(mouse);
        }

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

        public override void OnMouseWheelNoConsume(MouseState mouse)
        {
            if (!IsVisible) return;
            foreach (var element in Children)
            {
                element.OnMouseWheelNoConsume(mouse);
            }
            base.OnMouseWheelNoConsume(mouse);
        }

        public override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (!IsVisible) return;
            foreach (var element in Children)
            {
                element.OnKeyDown(e);
            }
        }

        public override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            if (!IsVisible) return;
            foreach (var element in Children)
            {
                element.OnKeyUp(e);
            }
        }

        public override void OnTextInput(TextInputEventArgs e)
        {
            if (!IsVisible) return;
            foreach (var element in Children)
            {
                element.OnTextInput(e);
            }
        }

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