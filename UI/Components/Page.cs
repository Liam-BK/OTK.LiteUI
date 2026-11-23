using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.UI.Rendering;

namespace OTK.LiteUI.UI.Components
{
    /// <summary>
    /// Represents a UI page that contains and manages child components.
    /// Handles input, updates, and rendering for its children.
    /// </summary>
    public class Page : UIElement, IRenderable, IUIContainer
    {
        /// <summary>
        /// The child components managed by this page.
        /// </summary>
        public List<UIComponent> Children { get; set; } = new List<UIComponent>();

        private bool _isVisible = true;

        /// <summary>
        /// Gets or sets whether the page and its contents are visible.
        /// </summary>
        [UIProperty]
        public override bool IsVisible
        {
            get => _isVisible;
            set
            {
                _isVisible = value;
            }
        }

        /// <summary>
        /// Initializes a page on the specified canvas.
        /// </summary>
        /// <param name="canvas">The canvas to register the page with.</param>
        public Page(UICanvas canvas)
        {
            Canvas = canvas;
            Canvas.Register(this);
            if (GetType() == typeof(Page)) Identifier = Canvas.GetNextIdentifier(typeof(Page));
        }

        /// <summary>
        /// Initializes a page on the default canvas.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no default canvas is assigned.
        /// </exception>
        public Page() : this(UICanvas.Default ?? throw new InvalidOperationException("Default Canvas must not be null for this Constructor. Try Creating a new UICanvas or assigning a UICanvas instance to UICanvas.Default")) { }

        /// <summary>
        /// Adds a component to the page and assigns the page as its parent.
        /// The component is also deregistered from the canvas.
        /// </summary>
        /// <param name="child">The component to add.</param>
        public void AddChild(UIComponent child)
        {
            Canvas.Deregister(child);
            child.Parent = this;
            Children.Add(child);
        }

        /// <summary>
        /// Removes all child components from the page.
        /// </summary>
        public void Clear()
        {
            Children.Clear();
        }

        /// <summary>
        /// Removes a component from the page.
        /// </summary>
        /// <param name="child">The component to remove.</param>
        public void RemoveChild(UIComponent child)
        {
            Children.Remove(child);
        }

        /// <summary>
        /// Does nothing because pages do not support layouts. Implemented to satisfy <see cref="IUIContainer"/>.
        /// </summary>
        /// <param name="layout">The layout to apply.</param>
        public void SetLayout(ILayout layout) { }

        /// <summary>
        /// Submits the renderable child components to the renderer.
        /// Does nothing when the page is hidden.
        /// </summary>
        /// <param name="renderer">The renderer receiving the child components' data.</param>
        public void SubmitData(Renderer renderer)
        {
            if (!IsVisible) return;
            foreach (var child in Children)
            {
                if (child is IRenderable renderable) renderable.SubmitData(renderer);
            }
        }

        /// <summary>
        /// Handles mouse button presses by forwarding them to child components.
        /// Moves the component that handles the press to the end of the child collection.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>True if a child component or the page handled the press.</returns>
        public override bool OnClickDown(MouseState mouse)
        {
            if (!IsVisible) return false;
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                var element = Children[i];
                if (element.OnClickDown(mouse))
                {
                    Children[i] = Children[^1];
                    Children[^1] = element;
                    return true;
                }
            }
            return base.OnClickDown(mouse);
        }

        /// <summary>
        /// Forwards mouse button presses to all child components without consuming the event, regardless of if it hits something.
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
        /// Forwards mouse button releases to child components until one handles the event.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>True if a child component or the page handled the release.</returns>
        public override bool OnClickUp(MouseState mouse)
        {
            if (!IsVisible) return false;
            foreach (var element in Children)
            {
                bool result = element.OnClickUp(mouse);
                if (result) return true;
            }
            return base.OnClickUp(mouse);
        }

        /// <summary>
        /// Forwards mouse button releases to all child components without consuming the event, regardless of if it hits something.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        public override void OnClickUpNoConsume(MouseState mouse)
        {
            if (!IsVisible) return;
            foreach (var element in Children)
            {
                element.OnClickUpNoConsume(mouse);
            }
            base.OnClickUpNoConsume(mouse);
        }

        /// <summary>
        /// Forwards keyboard key presses to all child components.
        /// </summary>
        /// <param name="e">The keyboard event arguments.</param>
        public override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (!IsVisible) return;
            foreach (var element in Children)
            {
                element.OnKeyDown(e);
            }
        }

        /// <summary>
        /// Forwards keyboard key releases to all child components.
        /// </summary>
        /// <param name="e">The keyboard event arguments.</param>
        public override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            if (!IsVisible) return;
            foreach (var element in Children)
            {
                element.OnKeyUp(e);
            }
        }

        /// <summary>
        /// Forwards text input to all child components.
        /// </summary>
        /// <param name="e">The text input event arguments.</param>
        public override void OnTextInput(TextInputEventArgs e)
        {
            if (!IsVisible) return;
            foreach (var element in Children)
            {
                element.OnTextInput(e);
            }
        }

        /// <summary>
        /// Forwards mouse movement to child components until one handles the event.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>True if a child component or the page handled the movement.</returns>
        public override bool OnMouseMove(MouseState mouse)
        {
            if (!IsVisible) return false;
            foreach (var element in Children)
            {
                bool result = element.OnMouseMove(mouse);
                if (result) return true;
            }
            return base.OnMouseMove(mouse);
        }

        /// <summary>
        /// Forwards mouse movement to all child components without consuming the event, regardless of if it hits something.
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
        /// Forwards mouse wheel input to child components until one handles the event.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>True if a child component or the page handled the input.</returns>
        public override bool OnMouseWheel(MouseState mouse)
        {
            if (!IsVisible) return false;
            foreach (var element in Children)
            {
                bool result = element.OnMouseWheel(mouse);
                if (result)
                {
                    return true;
                }
            }
            return base.OnMouseWheel(mouse);
        }

        /// <summary>
        /// Forwards mouse wheel input to all child components without consuming the event, regardless of if it hits something.
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
        /// Updates all child components while the page is visible.
        /// </summary>
        /// <param name="deltaTime">The elapsed time since the previous update.</param>
        /// <param name="mouse">The current mouse state.</param>
        /// <param name="keyboard">The current keyboard state.</param>
        public override void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
        {
            if (!IsVisible) return;
            foreach (var element in Children)
            {
                element.OnUpdate(deltaTime, mouse, keyboard);
            }
        }
    }
}