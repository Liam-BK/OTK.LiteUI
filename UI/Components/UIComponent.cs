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

    public virtual bool OnClickDown(MouseState mouse)
    {
        return true;
    }

    public virtual bool OnClickUp(MouseState mouse)
    {
        return true;
    }

    public virtual bool OnMouseMove(MouseState mouse)
    {
        return true;
    }

    public virtual bool OnMouseWheel(MouseState mouse)
    {
        return true;
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
        var position = UIScene.ConvertMouseScreenCoords(mouse.Position);
        return WithinBounds(position);
    }

    public bool WithinBounds(Vector2 position)
    {
        return position.X > Bounds.X && position.X <= Bounds.Z && position.Y > Bounds.Y && position.Y <= Bounds.W;
    }
}