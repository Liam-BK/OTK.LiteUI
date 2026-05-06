using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;


public enum ComponentOrientation
{
    Horizontal,
    Vertical
}

public abstract class UIComponent
{
    public static GameWindow? window = null;
    public virtual bool IsVisible { get; set; } = true;
    private IUIContainer? _parent = null;
    public IUIContainer? Parent { get => _parent; set => _parent = value; }

    public Vector4? ClipBounds = null;

    private Vector4 _bounds = Vector4.Zero;
    public virtual Vector4 Bounds { get => _bounds; set => _bounds = value; }

    public Vector2 Center => new Vector2((Bounds.X + Bounds.Z) * 0.5f, (Bounds.Y + Bounds.W) * 0.5f);

    public float Height => Bounds.W - Bounds.Y;

    public float Width => Bounds.Z - Bounds.X;

    protected Vector4 _colour = Vector4.One;

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

    public virtual bool CanFocus => false;

    public virtual void OnFocusGained() { }

    public virtual void OnFocusLost() { }

    public bool IsFocused => UIScene.FocusedComponent == this;

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

    public virtual bool WithinBounds(MouseState mouse)
    {
        return WithinBounds(UIScene.ConvertMouseScreenCoords(mouse.Position));
    }

    public bool WithinBounds(Vector2 position)
    {
        return position.X > Bounds.X && position.X <= Bounds.Z && position.Y > Bounds.Y && position.Y <= Bounds.W;
    }

    public static float MapValue(float value, float inputMin, float inputMax, float outputMin, float outputMax)
    {
        if (Math.Abs(inputMax - inputMin) < 0.0005f) return inputMin;
        return outputMin + (value - inputMin) * (outputMax - outputMin) / (inputMax - inputMin);
    }
}