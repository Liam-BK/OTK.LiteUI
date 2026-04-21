using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

/// <summary>
/// Represents a single UI element that can be added to a container.
/// Provides properties for positioning and sizing, as well as methods
/// for input handling, updating, and rendering.
/// </summary>
public interface IUIElement
{
    /// <summary>
    /// Gets or sets the parent container of this element.
    /// </summary>
    public IUIContainer? Parent
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the bounding rectangle of the element.
    /// Bounds are represented as (X = left, Y = bottom, Z = right, W = top).
    /// </summary>
    public Vector4 Bounds
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the center position of the element.
    /// </summary>
    public Vector2 Center
    {
        get;
    }

    /// <summary>
    /// Gets the height of the element (derived from bounds).
    /// </summary>
    public float Height
    {
        get;
    }

    /// <summary>
    /// Gets the width of the element (derived from bounds).
    /// </summary>
    public float Width
    {
        get;
    }

    /// <summary>
    /// Called when a mouse button is pressed while over the element.
    /// </summary>
    /// <param name="mouse">The current mouse state.</param>
    public bool OnClickDown(MouseState mouse);

    /// <summary>
    /// Called when a mouse button is released over the element.
    /// </summary>
    /// <param name="mouse">The current mouse state.</param>
    public bool OnClickUp(MouseState mouse);

    /// <summary>
    /// Called when the mouse wheel is scrolled.
    /// </summary>
    /// <param name="mouse">The current mouse state.</param>
    public bool OnMouseWheel(MouseState mouse);

    /// <summary>
    /// Called when the mouse moves.
    /// </summary>
    /// <param name="mouse">The current mouse state.</param>
    public bool OnMouseMove(MouseState mouse);

    /// <summary>
    /// Called when a key is pressed.
    /// </summary>
    /// <param name="e">The key event arguments.</param>
    public void OnKeyDown(KeyboardKeyEventArgs e);

    /// <summary>
    /// Called when a key is released.
    /// </summary>
    /// <param name="e">The key event arguments.</param>
    public void OnKeyUp(KeyboardKeyEventArgs e);

    /// <summary>
    /// Called when text input occurs while the element is focused.
    /// </summary>
    /// <param name="e">The text input event arguments.</param>
    public void OnTextInput(TextInputEventArgs e);

    /// <summary>
    /// Updates the element with delta time, mouse state, and keyboard state.
    /// </summary>
    /// <param name="deltaTime">Elapsed time since last update in seconds.</param>
    /// <param name="mouse">The current mouse state.</param>
    /// <param name="keyboard">The current keyboard state.</param>
    public void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard);

    /// <summary>
    /// Determines whether a given position is within the bounds of the element.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns><c>true</c> if the position is inside the element; otherwise, <c>false</c>.</returns>
    public bool WithinBounds(MouseState mouse);

    public bool WithinBounds(Vector2 position);
}