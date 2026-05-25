using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class Button : NineSlice
{
    protected Label label;
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

    public float TimeToRollover = 0.2f;

    private float _rolloverValue = 0;

    private bool _isHovered = false;

    protected float RolloverValue
    {
        get => _rolloverValue;
        set => _rolloverValue = value;
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

    public bool isPressed = false;

    public override Vector4 Colour
    {
        get => _baseColour;
        set
        {
            _baseColour = value;
        }
    }

    public override Vector4 Bounds
    {
        get
        {
            return base.Bounds;
        }
        set
        {
            base.Bounds = value;
            if (label is not null)
            {
                label.Origin = new Vector2((value.X + value.Z) * 0.5f, value.Y + (value.W - value.Y) * 0.25f);
                label.Size = (value.W - value.Y) * 0.5f;
            }
        }
    }

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
        UIScene.Deregister(label);
    }

    public override bool OnClickDown(MouseState mouse)
    {
        if (!IsVisible) return false;
        isPressed = WithinBounds(mouse);

        if (isPressed)
        {
            if (mouse.IsButtonDown(MouseButton.Left)) OnPointerDown?.Invoke(MouseButton.Left);
            else if (mouse.IsButtonDown(MouseButton.Right)) OnPointerDown?.Invoke(MouseButton.Right);
        }
        return isPressed;
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
        if (isPressed)
        {
            isPressed = WithinBounds(mouse);
        }
        return isPressed;
    }

    public override bool OnClickUp(MouseState mouse)
    {
        if (!IsVisible) return false;
        if (isPressed && WithinBounds(mouse))
        {
            if (mouse.IsButtonReleased(MouseButton.Left))
            {
                OnClick?.Invoke(MouseButton.Left);
            }
            else if (mouse.IsButtonReleased(MouseButton.Right))
            {
                OnClick?.Invoke(MouseButton.Right);
            }

            isPressed = false;
            return true;
        }
        return false;
    }

    public override void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
    {
        if (!IsVisible) return;
        var multiplier = new Vector4(isPressed ? 0.5f : 1.0f, isPressed ? 0.5f : 1.0f, isPressed ? 0.5f : 1.0f, 1);
        if (TimeToRollover == 0) RolloverValue = WithinBounds(mouse) ? 1 : 0;
        else RolloverValue = Math.Clamp(RolloverValue + deltaTime * (WithinBounds(mouse) ? 1 : -1) / TimeToRollover, 0, 1);
        var lerpColour = Vector4.Lerp(_baseColour, _rolloverColour, Math.Clamp(RolloverValue, 0, 1));
        ForceUpdateQuadrantColours(lerpColour * multiplier);
    }

    public override void SubmitData(InstanceRenderer renderer)
    {
        base.SubmitData(renderer);
        label.ClipBounds = ClipBounds;
        label.SubmitData(renderer);
    }
}