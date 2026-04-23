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

    protected float RolloverValue
    {
        get => _rolloverValue;
        set
        {
            _rolloverValue = value;
            float divisor = TimeToRollover > 0 ? TimeToRollover : 1;
            Colour = Vector4.Lerp(_baseColour, _rolloverColour, _rolloverValue / divisor);
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

    public Button(Vector4 bounds, string text = "", float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(bounds, inset, uvInset, colour)
    {
        _baseColour = colour ?? Vector4.One;
        label = new Label(new Vector2((bounds.X + bounds.Z) * 0.5f, bounds.Y + (bounds.W - bounds.Y) * 0.25f), (bounds.W - bounds.Y) * 0.5f, text);
        label.Alignment = TextAlignment.Center;
        label.Colour = new Vector4(0, 0, 0, 1);
    }

    public override bool OnClickDown(MouseState mouse)
    {
        return true;
    }

    public override bool OnClickUp(MouseState mouse)
    {
        return true;
    }

    public override void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
    {
        if (!IsVisible) return;
        if (WithinBounds(mouse))
        {
            if (TimeToRollover > 0) RolloverValue = Math.Min(TimeToRollover, RolloverValue + deltaTime);
            else Colour = _rolloverColour;
        }
        else
        {
            if (TimeToRollover > 0) RolloverValue = Math.Max(0, RolloverValue - deltaTime);
            else Colour = _baseColour;
        }
    }
}