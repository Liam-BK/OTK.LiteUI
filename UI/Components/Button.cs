using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class Button : NineSlice
{
    private Label label;
    public string Text
    {
        get
        {
            return label is null ? "" : label.Text;
        }
        set
        {
            if (label is not null) label.Text = value;
        }
    }

    public float TimeToRollover = 0.0f;

    private float _rolloverValue = 0;

    private bool _isHovered = false;

    protected float RolloverValue
    {
        get => _rolloverValue;
        set
        {
            _rolloverValue = value;
            float divisor = TimeToRollover > 0 ? TimeToRollover : 1;
            var multiplier = new Vector4(_isPressed ? 0.5f : 1.0f, _isPressed ? 0.5f : 1.0f, _isPressed ? 0.5f : 1.0f, 1);
            Colour = Vector4.Lerp(_baseColour, _rolloverColour, _rolloverValue / divisor) * multiplier;
        }
    }

    public override bool IsVisible
    {
        get => base.IsVisible;
        set
        {
            base.IsVisible = value;
            label.IsVisible = value;
        }
    }

    private Vector4 _rolloverColour = new Vector4(0.75f, 0.75f, 0.75f, 1);

    public Vector4 RolloverColour
    {
        set
        {
            _rolloverColour = value;
        }
    }

    private Vector4 _baseColour;

    public Vector4 TextColour
    {
        set
        {
            label.Colour = value;
        }
    }

    public bool _isPressed = false;

    public event Action<MouseButton>? OnPointerDown;

    public event Action<MouseButton>? OnClick;

    public event Action? OnHoverEnter;

    public event Action? OnHoverExit;

    public Button(Vector4 bounds, string text = "", float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(bounds, inset, uvInset, colour)
    {
        _baseColour = colour ?? Vector4.One;
        label = new Label(new Vector2((bounds.X + bounds.Z) * 0.5f, bounds.Y + (bounds.W - bounds.Y) * 0.25f), (bounds.W - bounds.Y) * 0.5f, text);
        label.Alignment = TextAlignment.Center;
        label.Colour = new Vector4(0, 0, 0, 1);
    }

    public override bool OnClickDown(MouseState mouse)
    {
        if (!IsVisible) return false;
        _isPressed = WithinBounds(mouse);

        if (_isPressed)
        {
            if (mouse.IsButtonDown(MouseButton.Left)) OnPointerDown?.Invoke(MouseButton.Left);
            else if (mouse.IsButtonDown(MouseButton.Right)) OnPointerDown?.Invoke(MouseButton.Right);
        }
        return _isPressed;
    }

    public override bool OnMouseMove(MouseState mouse)
    {
        if (!IsVisible) return false;
        var nextHoverState = WithinBounds(mouse);
        if (nextHoverState != _isHovered)
        {
            if (nextHoverState == false)
            {
                OnHoverExit?.Invoke();
            }
            else if (nextHoverState == true)
            {
                OnHoverEnter?.Invoke();
            }
            _isHovered = nextHoverState;
        }
        if (_isPressed)
        {
            _isPressed = WithinBounds(mouse);
        }
        return _isPressed;
    }

    public override bool OnClickUp(MouseState mouse)
    {
        if (!IsVisible) return false;
        if (_isPressed && WithinBounds(mouse))
        {
            if (mouse.IsButtonReleased(MouseButton.Left))
            {
                OnClick?.Invoke(MouseButton.Left);
            }
            else if (mouse.IsButtonReleased(MouseButton.Right))
            {
                OnClick?.Invoke(MouseButton.Right);
            }

            _isPressed = false;
            return true;
        }
        return false;
    }

    public override void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
    {
        if (!IsVisible) return;
        var multiplier = new Vector4(_isPressed ? 0.5f : 1.0f, _isPressed ? 0.5f : 1.0f, _isPressed ? 0.5f : 1.0f, 1);
        if (WithinBounds(mouse))
        {

            if (TimeToRollover > 0) RolloverValue = Math.Min(TimeToRollover, RolloverValue + deltaTime);
            else Colour = _rolloverColour * multiplier;
        }
        else
        {
            if (TimeToRollover > 0) RolloverValue = Math.Max(0, RolloverValue - deltaTime);
            else Colour = _baseColour * multiplier;
        }
    }
}