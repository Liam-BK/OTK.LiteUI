using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class Slider : NineSlice
{
    public override bool IsVisible
    {
        get => base.IsVisible;
        set
        {
            base.IsVisible = value;
            _thumb.IsVisible = value;
        }
    }
    public ComponentOrientation Orientation;
    private NineSlice _thumb;
    private Vector2 _clickOffset = Vector2.Zero;
    private Vector4 _thumbColour;
    private bool _thumbPressed = false;
    public string ThumbTexture
    {
        set
        {
            _thumb.Texture = value;
        }
    }
    private float _value = 0.0f;

    private float HalfThumbSize => Orientation == ComponentOrientation.Horizontal ? Height * 0.5f : Width * 0.5f;

    public float Value
    {
        get => _value;
        set
        {
            _value = Math.Clamp(value, 0, 1);
            SetThumbPositionFromValue();
            OnValueChanged?.Invoke(_value);
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
            if (_thumb is not null) SetThumbPositionFromValue();
        }
    }

    public event Action<float>? OnValueChanged;

    public override bool WithinBounds(MouseState mouse)
    {
        return base.WithinBounds(mouse) || _thumb.WithinBounds(mouse);
    }

    public Slider(Vector4 bounds, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(bounds, inset, uvInset, colour)
    {
        Orientation = Width >= Height ? ComponentOrientation.Horizontal : ComponentOrientation.Vertical;
        float halfThumbSize = HalfThumbSize;
        float x = (Orientation == ComponentOrientation.Horizontal) ? Bounds.X + halfThumbSize : Bounds.X + Width * 0.5f;
        float y = (Orientation == ComponentOrientation.Horizontal) ? Bounds.Y + Height * 0.5f : Bounds.Y + halfThumbSize;
        _thumb = new NineSlice(new Vector4(x - halfThumbSize, y - halfThumbSize, x + halfThumbSize, y + halfThumbSize));
        _thumbColour = colour ?? Vector4.One;
        UIScene.Deregister(_thumb);
    }

    private void SetValueFromThumbPosition()
    {
        var halfThumbSize = HalfThumbSize;
        if (Orientation == ComponentOrientation.Horizontal)
        {
            _value = (_thumb.Center.X - (Bounds.X + halfThumbSize)) / Math.Max(1, Width - halfThumbSize * 2);
            _value = Math.Clamp(_value, 0, 1);
            OnValueChanged?.Invoke(_value);
            return;
        }
        _value = (_thumb.Center.Y - (Bounds.Y + halfThumbSize)) / Math.Max(1, Height - halfThumbSize * 2);
        _value = Math.Clamp(_value, 0, 1);
        OnValueChanged?.Invoke(_value);
    }

    private void SetThumbPositionFromValue()
    {
        var halfThumbSize = HalfThumbSize;
        float x = (Orientation == ComponentOrientation.Horizontal) ? MathHelper.Lerp(Bounds.X + halfThumbSize, Bounds.Z - halfThumbSize, _value) : Bounds.X + Width * 0.5f;
        float y = (Orientation == ComponentOrientation.Horizontal) ? Bounds.Y + Height * 0.5f : MathHelper.Lerp(Bounds.Y + halfThumbSize, Bounds.W - halfThumbSize, _value);
        SetThumbPos(x, y);
    }

    private void SetThumbPos(float x, float y)
    {
        var halfThumbSize = HalfThumbSize;
        _thumb.Bounds = new Vector4(x - halfThumbSize, y - halfThumbSize, x + halfThumbSize, y + halfThumbSize);
    }

    public override bool OnClickDown(MouseState mouse)
    {
        if (!IsVisible) return false;
        var convertedMouse = UIScene.ConvertMouseScreenCoords(mouse.Position);
        if (_thumb.WithinBounds(mouse))
        {
            _thumbPressed = true;
            _clickOffset = convertedMouse - _thumb.Center;
            return true;
        }
        else if (!_thumb.WithinBounds(mouse) && WithinBounds(mouse))
        {
            float halfThumbSize = Orientation == ComponentOrientation.Horizontal ? Height * 0.55f : Width * 0.55f;
            if (Orientation == ComponentOrientation.Horizontal)
            {
                var x = convertedMouse.X;
                var y = Center.Y;
                x += ((x < _thumb.Center.X) ? 1 : -1) * halfThumbSize;
                SetThumbPos(x, y);
            }
            else
            {
                var x = Center.X;
                var y = convertedMouse.Y;
                y += ((y < _thumb.Center.Y) ? 1 : -1) * halfThumbSize;
                SetThumbPos(x, y);
            }
            _clickOffset = convertedMouse - _thumb.Center;
            _thumbPressed = true;
            SetValueFromThumbPosition();
        }
        return base.OnClickDown(mouse);
    }

    public override bool OnMouseMove(MouseState mouse)
    {
        if (!IsVisible) return false;
        var convertedMouse = UIScene.ConvertMouseScreenCoords(mouse.Position);
        if (_thumbPressed)
        {
            var halfThumbSize = HalfThumbSize;
            if (Orientation == ComponentOrientation.Horizontal)
            {
                float x = Math.Clamp(convertedMouse.X - _clickOffset.X, Bounds.X + halfThumbSize, Bounds.Z - halfThumbSize);
                float y = Center.Y;
                SetThumbPos(x, y);
                SetValueFromThumbPosition();
            }
            else
            {
                float x = Center.X;
                float y = Math.Clamp(convertedMouse.Y - _clickOffset.Y, Bounds.Y + halfThumbSize, Bounds.W - halfThumbSize);
                SetThumbPos(x, y);
                SetValueFromThumbPosition();
            }
            return true;
        }
        return base.OnMouseMove(mouse);
    }

    public override bool OnClickUp(MouseState mouse)
    {
        if (!IsVisible) return false;
        _thumbPressed = false;
        _clickOffset = Vector2.Zero;
        return base.OnClickUp(mouse);
    }

    public override void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
    {
        if (!IsVisible) return;
        base.OnUpdate(deltaTime, mouse, keyboard);
        _thumb.Colour = _thumbPressed ? _thumbColour * new Vector4(0.5f, 0.5f, 0.5f, 1.0f) : _thumbColour;
    }

    public override void SubmitData(InstanceRenderer renderer)
    {
        if (!IsVisible) return;
        base.SubmitData(renderer);
        _thumb.ClipBounds = ClipBounds;
        _thumb.SubmitData(renderer);
    }
}
