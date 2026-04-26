using OpenTK.Mathematics;

public class ScrollBar : NineSlice
{
    private NineSlice _thumb;
    public ScrollBar(Vector4 bounds, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(bounds, inset, uvInset, colour)
    {
        _thumb = new NineSlice(new Vector4());
    }
}