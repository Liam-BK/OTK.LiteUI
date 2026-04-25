using OpenTK.Mathematics;

public class StatusBar : NineSlice
{
    private UIQuad FillTopLeft = new UIQuad();
    private UIQuad FillTopCenter = new UIQuad();
    private UIQuad FillTopRight = new UIQuad();
    private UIQuad FillCenterLeft = new UIQuad();
    private UIQuad FillCenter = new UIQuad();
    private UIQuad FillCenterRight = new UIQuad();
    private UIQuad FillBottomLeft = new UIQuad();
    private UIQuad FillBottomCenter = new UIQuad();
    private UIQuad FillBottomRight = new UIQuad();
    public override Vector4 Bounds
    {
        get => base.Bounds;
        set
        {
            base.Bounds = value;
            UpdateFill();
        }
    }
    private float _fillAmount = 0.0f;
    public float FillAmount
    {
        get
        {
            return _fillAmount;
        }
        set
        {
            _fillAmount = Math.Clamp(value, 0, 1);
            UpdateFill();
        }
    }

    private string _fillTexture = "";

    public string FillTexture
    {
        set
        {
            _fillTexture = value;
            int textureLayer;
            if (_fillTexture == "") textureLayer = -1;
            else TextureManager.TryGetTexture(_fillTexture, UIScene.resolution, out textureLayer);
            FillTopLeft.textureLayer = textureLayer;
            FillTopCenter.textureLayer = textureLayer;
            FillTopRight.textureLayer = textureLayer;
            FillCenterLeft.textureLayer = textureLayer;
            FillCenter.textureLayer = textureLayer;
            FillCenterRight.textureLayer = textureLayer;
            FillBottomLeft.textureLayer = textureLayer;
            FillBottomCenter.textureLayer = textureLayer;
            FillBottomRight.textureLayer = textureLayer;
        }
    }

    private Vector4 _fillColour = Vector4.One;

    public Vector4 FillColour
    {
        set
        {
            _fillColour = value;
            FillTopLeft.colour = _fillColour;
            FillTopCenter.colour = _fillColour;
            FillTopRight.colour = _fillColour;
            FillCenterLeft.colour = _fillColour;
            FillCenter.colour = _fillColour;
            FillCenterRight.colour = _fillColour;
            FillBottomLeft.colour = _fillColour;
            FillBottomCenter.colour = _fillColour;
            FillBottomRight.colour = _fillColour;
        }
    }

    public StatusBar(Vector4 bounds, float fillAmount = 0, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(bounds, inset, uvInset, colour)
    {
        FillAmount = fillAmount;
        FillTopLeft.colour = _fillColour;
        FillTopCenter.colour = _fillColour;
        FillTopRight.colour = _fillColour;
        FillCenterLeft.colour = _fillColour;
        FillCenter.colour = _fillColour;
        FillCenterRight.colour = _fillColour;
        FillBottomLeft.colour = _fillColour;
        FillBottomCenter.colour = _fillColour;
        FillBottomRight.colour = _fillColour;
        int textureLayer = -1;
        FillTopLeft.textureLayer = textureLayer;
        FillTopCenter.textureLayer = textureLayer;
        FillTopRight.textureLayer = textureLayer;
        FillCenterLeft.textureLayer = textureLayer;
        FillCenter.textureLayer = textureLayer;
        FillCenterRight.textureLayer = textureLayer;
        FillBottomLeft.textureLayer = textureLayer;
        FillBottomCenter.textureLayer = textureLayer;
        FillBottomRight.textureLayer = textureLayer;
    }

    private void UpdateFill()
    {
        float multiplier1 = Math.Clamp(FillAmount / (Inset / Width), 0f, 1f);
        float multiplier2 = Math.Clamp((FillAmount - Inset / Width) / ((Width - 2 * Inset) / Width), 0, 1);
        float multiplier3 = Math.Clamp((FillAmount * Width - (Width - Inset)) / Inset, 0, 1);
        FillTopLeft.position = new Vector2(Bounds.X + Inset * 0.5f * multiplier1, Bounds.W - Inset * 0.5f);
        FillCenterLeft.position = new Vector2(Bounds.X + Inset * 0.5f * multiplier1, Bounds.Y + Height * 0.5f);
        FillBottomLeft.position = new Vector2(Bounds.X + Inset * 0.5f * multiplier1, Bounds.Y + Inset * 0.5f);

        FillTopCenter.position = new Vector2(Bounds.X + Inset + (Width - Inset * 2) * 0.5f * multiplier2, Bounds.W - Inset * 0.5f);
        FillCenter.position = new Vector2(Bounds.X + Inset + (Width - Inset * 2) * 0.5f * multiplier2, Bounds.Y + Height * 0.5f);
        FillBottomCenter.position = new Vector2(Bounds.X + Inset + (Width - Inset * 2) * 0.5f * multiplier2, Bounds.Y + Inset * 0.5f);

        FillTopRight.position = new Vector2(Bounds.Z - Inset + Inset * 0.5f * multiplier3, Bounds.W - Inset * 0.5f);
        FillBottomRight.position = new Vector2(Bounds.Z - Inset + Inset * 0.5f * multiplier3, Bounds.Y + Inset * 0.5f);
        FillCenterRight.position = new Vector2(Bounds.Z - Inset + Inset * 0.5f * multiplier3, Bounds.Y + Height * 0.5f);

        FillTopLeft.size = new Vector2(Inset * multiplier1, Inset);
        FillCenterLeft.size = new Vector2(Inset * multiplier1, Height - Inset * 2);
        FillBottomLeft.size = new Vector2(Inset * multiplier1, Inset);

        FillTopCenter.size = new Vector2((Width - Inset * 2) * multiplier2, Inset);
        FillCenter.size = new Vector2((Width - Inset * 2) * multiplier2, Height - Inset * 2);
        FillBottomCenter.size = new Vector2((Width - Inset * 2) * multiplier2, Inset);

        FillTopRight.size = new Vector2(Inset * multiplier3, Inset);
        FillBottomRight.size = new Vector2(Inset * multiplier3, Inset);
        FillCenterRight.size = new Vector2(Inset * multiplier3, Inset);

        FillTopLeft.UVOffset = new Vector2(0, 1 - UVInset);
        FillCenterLeft.UVOffset = new Vector2(0, UVInset);
        FillBottomLeft.UVOffset = new Vector2(0, 0);

        FillTopCenter.UVOffset = new Vector2(UVInset, 1 - UVInset);
        FillCenter.UVOffset = new Vector2(UVInset, UVInset);
        FillBottomCenter.UVOffset = new Vector2(UVInset, 0);

        FillTopRight.UVOffset = new Vector2(1 - UVInset, 1 - UVInset);
        FillCenterRight.UVOffset = new Vector2(1 - UVInset, UVInset);
        FillBottomRight.UVOffset = new Vector2(1 - UVInset, 0);

        FillTopLeft.UVRange = new Vector2(UVInset * multiplier1, UVInset);
        FillCenterLeft.UVRange = new Vector2(UVInset * multiplier1, 1 - UVInset * 2);
        FillBottomLeft.UVRange = new Vector2(UVInset * multiplier1, UVInset);

        FillTopCenter.UVRange = new Vector2((1 - UVInset * 2) * multiplier2, UVInset);
        FillCenter.UVRange = new Vector2((1 - UVInset * 2) * multiplier2, 1 - UVInset * 2);
        FillBottomCenter.UVRange = new Vector2((1 - UVInset * 2) * multiplier2, UVInset);

        FillTopRight.UVRange = new Vector2(UVInset * multiplier3, UVInset);
        FillCenterRight.UVRange = new Vector2(UVInset * multiplier3, 1 - UVInset * 2);
        FillBottomRight.UVRange = new Vector2(UVInset * multiplier3, UVInset);

        // Fill.UVOffset = new Vector2(0, 0);
        // Fill.UVRange = new Vector2(FillAmount, 1);
    }

    public override void SubmitData(InstanceRenderer renderer)
    {
        base.SubmitData(renderer);
        if (FillAmount > 0)
        {
            renderer.AddInstance(FillTopLeft);
            renderer.AddInstance(FillCenterLeft);
            renderer.AddInstance(FillBottomLeft);
        }
        if (FillAmount > (Inset / Width))
        {
            renderer.AddInstance(FillTopCenter);
            renderer.AddInstance(FillCenter);
            renderer.AddInstance(FillBottomCenter);
        }
        if (FillAmount > ((Width - Inset) / Width))
        {
            renderer.AddInstance(FillTopRight);
            renderer.AddInstance(FillCenterRight);
            renderer.AddInstance(FillBottomRight);
        }
    }
}