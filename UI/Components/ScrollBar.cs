using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class ScrollBar : NineSlice
{
    public ComponentOrientation Orientation;

    private NineSlice _thumb;

    private Vector2 _contentSize = Vector2.One;

    public Vector2 ContentSize
    {
        get
        {
            return _contentSize;
        }
        set
        {
            _contentSize = value;
            UpdateThumbFromValue();
        }
    }

    private Vector2 _viewSize = Vector2.One;

    public Vector2 ViewSize
    {
        get
        {
            return _viewSize;
        }
        set
        {
            _viewSize = value;
            UpdateThumbFromValue();
        }
    }

    private float _value = 0.0f;

    public float Value
    {
        get
        {
            return _value;
        }
        set
        {
            _value = Math.Clamp(value, 0, 1);
            UpdateThumbFromValue();
        }
    }

    public string ThumbTexture
    {
        set
        {
            _thumb.Texture = value;
        }
    }

    private Vector4 _thumbColour;

    public Vector4 ThumbColour
    {
        get
        {
            return _thumbColour;
        }
        set
        {
            _thumbColour = value;
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
            Orientation = Width >= Height ? ComponentOrientation.Horizontal : ComponentOrientation.Vertical;
            if (_thumb is not null) UpdateThumbFromValue();
        }
    }

    public const float scrollSensitivity = 0.4f;

    private bool _isPressed = false;

    private Vector2 _clickOffset = Vector2.Zero;

    public ScrollBar(Vector4 bounds, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(bounds, inset, uvInset, colour)
    {
        Orientation = Width >= Height ? ComponentOrientation.Horizontal : ComponentOrientation.Vertical;
        _thumb = new NineSlice(bounds);
        _thumbColour = colour ?? Vector4.One;
        UIScene.Deregister(_thumb);
    }

    private void UpdateThumbFromValue()
    {
        if (_contentSize.X == 0 || _contentSize.Y == 0) return;
        Vector2 proportion = new Vector2(Math.Min(_viewSize.X / _contentSize.X, 1.0f), Math.Min(_viewSize.Y / _contentSize.Y, 1.0f));
        float thumbWidth = Orientation == ComponentOrientation.Horizontal ? Width * proportion.X : Width;
        float thumbHeight = Orientation == ComponentOrientation.Vertical ? Height * proportion.Y : Height;
        float maxOffset = 1 - (Orientation == ComponentOrientation.Horizontal ? proportion.X : proportion.Y);
        float left = Orientation == ComponentOrientation.Horizontal ? Bounds.X + maxOffset * _value * Width : Bounds.X;
        float right = Orientation == ComponentOrientation.Horizontal ? left + thumbWidth : Bounds.Z;
        float top = Orientation == ComponentOrientation.Vertical ? Bounds.W - maxOffset * _value * Height : Bounds.W;
        float bottom = Orientation == ComponentOrientation.Vertical ? top - thumbHeight : Bounds.Y;
        _thumb.Bounds = new Vector4(left, bottom, right, top);
    }

    private void UpdateValueFromThumb()
    {
        if (Orientation == ComponentOrientation.Horizontal)
        {
            var denominator = Math.Max(Width - _thumb.Width, 0.0f);
            _value = Math.Abs(denominator) < 0.0005f ? 0 : (_thumb.Bounds.X - Bounds.X) / denominator;
        }
        else
        {
            var denominator = Math.Max(Height - _thumb.Height, 0.0f);
            _value = Math.Abs(denominator) < 0.0005f ? 0 : (Bounds.W - _thumb.Bounds.W) / denominator;
        }
    }

    private void SetThumbPos(float x, float y)
    {
        Vector2 halfThumbSize = new Vector2(_thumb.Width, _thumb.Height) * 0.5f;
        _thumb.Bounds = new Vector4(x - halfThumbSize.X, y - halfThumbSize.Y, x + halfThumbSize.X, y + halfThumbSize.Y);
    }

    public override bool OnClickDown(MouseState mouse)
    {
        if (!IsVisible) return false;
        var convertedMouse = UIScene.ConvertMouseScreenCoords(mouse.Position);
        if (_thumb.WithinBounds(mouse))
        {
            _isPressed = true;
            _clickOffset = convertedMouse - _thumb.Center;
        }
        else if (WithinBounds(mouse))
        {
            if (Orientation == ComponentOrientation.Horizontal)
            {
                float difference = convertedMouse.X - _thumb.Center.X;
                float halfSize = _thumb.Width * 0.5f * (difference > 0 ? 1 : -1);
                SetThumbPos(_thumb.Center.X + difference - halfSize, _thumb.Center.Y);
            }
            else
            {
                float difference = convertedMouse.Y - _thumb.Center.Y;
                float halfSize = _thumb.Height * 0.5f * (difference > 0 ? 1 : -1);
                SetThumbPos(_thumb.Center.X, _thumb.Center.Y + difference - halfSize);
            }
            _isPressed = true;
            _clickOffset = convertedMouse - _thumb.Center;
            UpdateValueFromThumb();
        }
        return _isPressed;
    }

    public override bool OnMouseMove(MouseState mouse)
    {
        if (_isPressed)
        {
            var convertedMouse = UIScene.ConvertMouseScreenCoords(mouse.Position);
            if (Orientation == ComponentOrientation.Horizontal)
            {
                SetThumbPos(Math.Clamp(convertedMouse.X - _clickOffset.X, Bounds.X + _thumb.Width * 0.5f, Bounds.Z - _thumb.Width * 0.5f), Center.Y);
            }
            if (Orientation == ComponentOrientation.Vertical)
            {
                SetThumbPos(Center.X, Math.Clamp(convertedMouse.Y - _clickOffset.Y, Bounds.Y + _thumb.Height * 0.5f, Bounds.W - _thumb.Height * 0.5f));
            }
            UpdateValueFromThumb();
            return true;
        }
        return base.OnMouseMove(mouse);
    }

    public override bool OnClickUp(MouseState mouse)
    {
        _isPressed = false;
        _clickOffset = Vector2.Zero;
        return base.OnClickUp(mouse);
    }

    public override bool OnMouseWheel(MouseState mouse)
    {
        if (!IsVisible) return false;
        if (WithinBounds(mouse))
        {
            if (Orientation == ComponentOrientation.Horizontal)
            {
                float velocity = mouse.ScrollDelta.X * scrollSensitivity;
                SetThumbPos(Math.Clamp(_thumb.Center.X - velocity, Bounds.X + _thumb.Width * 0.5f, Bounds.Z - _thumb.Width * 0.5f), Center.Y);
                UpdateValueFromThumb();
                return true;
            }
            else
            {
                float velocity = mouse.ScrollDelta.Y * scrollSensitivity;
                SetThumbPos(Center.X, Math.Clamp(_thumb.Center.Y + velocity, Bounds.Y + _thumb.Height * 0.5f, Bounds.W - _thumb.Height * 0.5f));
                UpdateValueFromThumb();
                return true;
            }
        }
        return false;
    }

    public override void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
    {
        _thumb.Colour = _thumbColour * (_isPressed ? 0.5f : 1.0f);
    }

    public override void SubmitData(InstanceRenderer renderer)
    {
        if (!IsVisible) return;
        base.SubmitData(renderer);
        _thumb.ClipBounds = ClipBounds;
        _thumb.SubmitData(renderer);
    }
}