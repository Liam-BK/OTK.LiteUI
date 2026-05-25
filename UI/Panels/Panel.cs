using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Rendering;

namespace OTK.LiteUI.UI.Components
{
    public class Panel : NineSlice, IUIContainer, IScrollable
    {
        private List<UIComponent> _children = new List<UIComponent>();

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

        public Panel(Vector4 bounds, ILayout layout, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(bounds, inset, uvInset, colour)
        {
            Layout = layout;
        }

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

        public void AddChild(UIComponent child)
        {
            UIScene.Deregister(child);
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

        public override bool OnClickDown(MouseState mouse)
        {
            if (!IsVisible || !WithinBounds(mouse)) return false;
            foreach (var element in Children)
            {
                bool result = element.OnClickDown(mouse);
                if (result) return true;
            }
            return base.OnClickDown(mouse);
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

        public override bool OnMouseMove(MouseState mouse)
        {
            if (!IsVisible || !WithinBounds(mouse)) return false;
            foreach (var element in Children)
            {
                bool result = element.OnMouseMove(mouse);
                if (result) return true;
            }
            return base.OnMouseMove(mouse);
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
            ScrollOffset = new Vector2(ScrollOffset.X - mouse.ScrollDelta.X, ScrollOffset.Y - mouse.ScrollDelta.Y);
            ApplyLayout();
            return base.OnMouseWheel(mouse);
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
                element.OnUpdate(deltaTime, mouse, keyboard);
            }
        }

        public override void SubmitData(Renderer renderer)
        {
            if (!IsVisible) return;
            base.SubmitData(renderer);
            foreach (var element in Children)
            {
                if (element is IRenderable renderable)
                {
                    renderable.SubmitData(renderer);
                }
            }
        }
    }
}