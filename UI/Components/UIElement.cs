using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OTK.LiteUI.Core;

namespace OTK.LiteUI.UI.Components
{
    /// <summary>
    /// Provides the base functionality for UI elements, including visibility,
    /// identification, input handling, and update callbacks.
    /// </summary>
    public abstract class UIElement
    {
        /// <summary>
        /// The canvas this element belongs to.
        /// </summary>
        public UICanvas Canvas = null!;

        /// <summary>
        /// Gets or sets whether the element is visible.
        /// </summary>
        [UIProperty]
        public virtual bool IsVisible { get; set; } = true;

        /// <summary>
        /// Gets or sets the unique identifier used to reference this element.
        /// </summary>
        [UIProperty]
        public string Identifier
        {
            get;
            set;
        } = "";

        /// <summary>
        /// Handles a mouse button press.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>
        /// <see langword="true"/> if the input was handled; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public virtual bool OnClickDown(MouseState mouse)
        {
            return false;
        }

        /// <summary>
        /// Handles a mouse button press without consuming the input.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        public virtual void OnClickDownNoConsume(MouseState mouse)
        {

        }

        /// <summary>
        /// Handles a mouse button release.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>
        /// <see langword="true"/> if the input was handled; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public virtual bool OnClickUp(MouseState mouse)
        {
            return false;
        }

        /// <summary>
        /// Handles a mouse button release without consuming the input.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        public virtual void OnClickUpNoConsume(MouseState mouse)
        {

        }

        /// <summary>
        /// Handles mouse movement.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>
        /// <see langword="true"/> if the input was handled; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public virtual bool OnMouseMove(MouseState mouse)
        {
            return false;
        }

        /// <summary>
        /// Handles mouse movement without consuming the input.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        public virtual void OnMouseMoveNoConsume(MouseState mouse)
        {

        }

        /// <summary>
        /// Handles mouse wheel input.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        /// <returns>
        /// <see langword="true"/> if the input was handled; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public virtual bool OnMouseWheel(MouseState mouse)
        {
            return false;
        }

        /// <summary>
        /// Handles mouse wheel input without consuming the input.
        /// </summary>
        /// <param name="mouse">The current mouse state.</param>
        public virtual void OnMouseWheelNoConsume(MouseState mouse)
        {

        }

        /// <summary>
        /// Handles text input.
        /// </summary>
        /// <param name="e">The text input event.</param>
        public virtual void OnTextInput(TextInputEventArgs e)
        {

        }

        /// <summary>
        /// Handles a keyboard key press.
        /// </summary>
        /// <param name="e">The keyboard event.</param>
        public virtual void OnKeyDown(KeyboardKeyEventArgs e)
        {

        }

        /// <summary>
        /// Handles a keyboard key release.
        /// </summary>
        /// <param name="e">The keyboard event.</param>
        public virtual void OnKeyUp(KeyboardKeyEventArgs e)
        {

        }

        /// <summary>
        /// Updates the element.
        /// </summary>
        /// <param name="deltaTime">The elapsed time since the previous update.</param>
        /// <param name="mouse">The current mouse state.</param>
        /// <param name="keyboard">The current keyboard state.</param>
        public virtual void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
        {

        }

        /// <summary>
        /// Removes this element from the specified registry.
        /// </summary>
        /// <param name="registry">The registry containing this element.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown when this element is not registered.
        /// </exception>
        public virtual void Deregister(List<UIElement> registry)
        {
            if (!registry.Remove(this)) throw new InvalidOperationException("Attempted to deregister a UI element that was not registered.");
        }
    }
}