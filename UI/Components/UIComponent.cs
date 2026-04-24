using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

public abstract class UIComponent : IUIElement
{
    public static GameWindow? window = null;
    public bool IsVisible { get; set; } = true;
    private IUIContainer? _parent = null;
    public IUIContainer? Parent { get => _parent; set => _parent = value; }

    private Vector4 _bounds = Vector4.Zero;
    public Vector4 Bounds { get => _bounds; set => _bounds = value; }

    public Vector2 Center => new Vector2((_bounds.X + _bounds.Z) * 0.5f, (_bounds.Y + _bounds.W) * 0.5f);

    public float Height => _bounds.W - _bounds.Y;

    public float Width => _bounds.Z - _bounds.X;

    protected Vector4 _colour = Vector4.One;

    public virtual Vector4 Colour
    {
        get;
        set;
    }

    public virtual bool OnClickDown(MouseState mouse)
    {
        return false;
    }

    public virtual bool OnClickUp(MouseState mouse)
    {
        return false;
    }

    public virtual bool OnMouseMove(MouseState mouse)
    {
        return false;
    }

    public virtual bool OnMouseWheel(MouseState mouse)
    {
        return false;
    }

    public virtual void OnTextInput(TextInputEventArgs e)
    {

    }

    public virtual void OnKeyDown(KeyboardKeyEventArgs e)
    {

    }

    public virtual void OnKeyUp(KeyboardKeyEventArgs e)
    {

    }

    public virtual void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
    {

    }

    public bool WithinBounds(MouseState mouse)
    {
        return WithinBounds(UIScene.ConvertMouseScreenCoords(mouse.Position));
    }

    public bool WithinBounds(Vector2 position)
    {
        return position.X > Bounds.X && position.X <= Bounds.Z && position.Y > Bounds.Y && position.Y <= Bounds.W;
    }
}