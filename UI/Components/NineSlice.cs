using OpenTK.Mathematics;

public class NineSlice : UIComponent, IRenderable
{
    public string Texture { private get; set; } = "";
    private float _inset;
    public float Inset
    {
        get => _inset;
        set
        {
            var min = Math.Min(Width, Height);
            if (min < value * 2)
            {
                _inset = min * 0.5f;
            }
            else
            {
                _inset = value;
            }
        }
    }
    private float _uvInset;
    public float UVInset
    {
        get
        {
            return _uvInset;
        }
        set
        {
            _uvInset = Math.Clamp(value, 0, 0.5f);
        }
    }
    private UIQuad topLeft = new UIQuad();
    private UIQuad topCenter = new UIQuad();
    private UIQuad topRight = new UIQuad();
    private UIQuad centerLeft = new UIQuad();
    private UIQuad center = new UIQuad();
    private UIQuad centerRight = new UIQuad();
    private UIQuad bottomLeft = new UIQuad();
    private UIQuad bottomCenter = new UIQuad();
    private UIQuad bottomRight = new UIQuad();

    public NineSlice(Vector4 bounds, float inset = 10, float uvInset = 0.25f, Vector4? colour = null)
    {
        UIScene.Register(this);
        Bounds = bounds;
        Inset = inset;
        UVInset = uvInset;
        Colour = colour ?? Vector4.One;
        UpdateQuadrants();
    }

    private void UpdateQuadrants()
    {
        int textureLayer;
        if (Texture == "") textureLayer = -1;
        else TextureManager.TryGetTexture(Texture, UIScene.resolution, out textureLayer);

        topLeft.position = new Vector2(Bounds.X + Inset * 0.5f, Bounds.W - Inset * 0.5f);
        topLeft.size = new Vector2(Inset, Inset);
        topLeft.UVOffset = new Vector2(0, 1 - UVInset);
        topLeft.UVRange = new Vector2(UVInset, UVInset);
        topLeft.colour = Colour;
        topLeft.textureLayer = textureLayer;

        topCenter.position = new Vector2(Center.X, Bounds.W - Inset * 0.5f);
        topCenter.size = new Vector2(Width - (Inset * 2), Inset);
        topCenter.UVOffset = new Vector2(UVInset, 1 - UVInset);
        topCenter.UVRange = new Vector2(1 - (UVInset * 2), UVInset);
        topCenter.colour = Colour;
        topCenter.textureLayer = textureLayer;

        topRight.position = new Vector2(Bounds.Z - Inset * 0.5f, Bounds.W - Inset * 0.5f);
        topRight.size = new Vector2(Inset, Inset);
        topRight.UVOffset = new Vector2(1 - UVInset, 1 - UVInset);
        topRight.UVRange = new Vector2(UVInset, UVInset);
        topRight.colour = Colour;
        topRight.textureLayer = textureLayer;

        centerLeft.position = new Vector2(Bounds.X + Inset * 0.5f, Center.Y);
        centerLeft.size = new Vector2(Inset, Height - Inset * 2);
        centerLeft.UVOffset = new Vector2(0, UVInset);
        centerLeft.UVRange = new Vector2(UVInset, 1 - UVInset * 2);
        centerLeft.colour = Colour;
        centerLeft.textureLayer = textureLayer;

        center.position = new Vector2(Center.X, Center.Y);
        center.size = new Vector2(Width - (Inset * 2), Height - Inset * 2);
        center.UVOffset = new Vector2(UVInset, UVInset);
        center.UVRange = new Vector2(1 - (UVInset * 2), 1 - UVInset * 2);
        center.colour = Colour;
        center.textureLayer = textureLayer;

        centerRight.position = new Vector2(Bounds.Z - Inset * 0.5f, Center.Y);
        centerRight.size = new Vector2(Inset, Height - Inset * 2);
        centerRight.UVOffset = new Vector2(1 - UVInset, UVInset);
        centerRight.UVRange = new Vector2(UVInset, 1 - UVInset * 2);
        centerRight.colour = Colour;
        centerRight.textureLayer = textureLayer;

        bottomLeft.position = new Vector2(Bounds.X + Inset * 0.5f, Bounds.Y + Inset * 0.5f);
        bottomLeft.size = new Vector2(Inset, Inset);
        bottomLeft.UVOffset = new Vector2(0, 0);
        bottomLeft.UVRange = new Vector2(UVInset, UVInset);
        bottomLeft.colour = Colour;
        bottomLeft.textureLayer = textureLayer;

        bottomCenter.position = new Vector2(Center.X, Bounds.Y + Inset * 0.5f);
        bottomCenter.size = new Vector2(Width - (Inset * 2), Inset);
        bottomCenter.UVOffset = new Vector2(UVInset, 0);
        bottomCenter.UVRange = new Vector2(1 - (UVInset * 2), UVInset);
        bottomCenter.colour = Colour;
        bottomCenter.textureLayer = textureLayer;

        bottomRight.position = new Vector2(Bounds.Z - Inset * 0.5f, Bounds.Y + Inset * 0.5f);
        bottomRight.size = new Vector2(Inset, Inset);
        bottomRight.UVOffset = new Vector2(1 - UVInset, 0);
        bottomRight.UVRange = new Vector2(UVInset, UVInset);
        bottomRight.colour = Colour;
        bottomRight.textureLayer = textureLayer;
    }

    public virtual void SubmitData(InstanceRenderer renderer)
    {
        if (!IsVisible) return;
        UpdateQuadrants();
        renderer.AddInstance(topLeft);
        renderer.AddInstance(topCenter);
        renderer.AddInstance(topRight);
        renderer.AddInstance(centerLeft);
        renderer.AddInstance(center);
        renderer.AddInstance(centerRight);
        renderer.AddInstance(bottomLeft);
        renderer.AddInstance(bottomCenter);
        renderer.AddInstance(bottomRight);
    }
}