using OpenTK.Mathematics;

public interface ILayout
{
    public float MaxScrollInsetMultiplierX
    {
        get;
    }

    public float MaxScrollInsetMultiplierY
    {
        get;
    }

    public float Padding
    {
        get;
        set;
    }

    public Vector2 ElementSize
    {
        get;
        set;
    }

    public void Apply(Vector4 viewport, List<UIComponent> elements);
}