using OpenTK.Mathematics;

public enum TextAlignment
{
    Left,
    Center,
    Right
}

public class Label : UIComponent, IRenderable
{
    private Vector2 _origin = Vector2.Zero;
    public Vector2 Origin
    {
        get => _origin;
        set
        {
            _origin = value;
            _isDirty = true;
        }
    }

    private float _size = 0;
    public float Size
    {
        get => _size;
        set
        {
            _size = value;
            _isDirty = true;
        }
    }
    public TextAlignment Alignment = TextAlignment.Left;
    public int InstanceCount => _glyphs.Count;
    private List<UIQuad> _glyphs = new List<UIQuad>();
    private float _lineSpacing = 0.0f;
    public string FontKey
    {
        private get;
        set;
    }
    private string _text = "";
    public string Text
    {
        get => _text;
        set
        {
            if (value != _text)
            {
                _text = value;
                _isDirty = true;
            }
        }
    }

    private bool _isDirty = true;

    public Label(Vector2 origin, float size, string text = "", Vector4? colour = null)
    {
        Origin = origin;
        Size = size;
        Colour = colour ?? Vector4.One;
        Text = text;
        FontKey = FontManager.DefaultFontKey;
        Console.WriteLine(FontKey);
        TextureManager.TryGetTexture(FontKey, UIScene.resolution, out var layer);
        Console.WriteLine($"layer: {layer}");
        UIScene.Register(this);
    }

    private void UpdateGlyphs()
    {
        _glyphs.Clear();
        float XCursor = Origin.X;
        float YCursor = Origin.Y;
        if (!FontManager.Fonts.TryGetValue(FontKey, out var fontData))
        {
            Console.WriteLine("No FontData");
            return;
        }
        for (int i = 0; i < Text.Length; i++)
        {
            char c = Text[i];
            if (c == '\n')
            {
                YCursor -= Size + _lineSpacing;
                XCursor = Origin.X;
                continue;
            }
            if (!fontData.GlyphUVs.TryGetValue(c, out var UVs)) continue;
            if (!fontData.GlyphBounds.TryGetValue(c, out var charBounds)) continue;
            if (!fontData.Offsets.TryGetValue(c, out var offset)) continue;
            float kern = 0.0f;
            if (i < Text.Length - 1)
            {
                var pair = (c, Text[i + 1]);
                if (!fontData.Kerning.TryGetValue(pair, out kern))
                {
                    kern = 0.0f;
                }
            }
            offset /= fontData.ScaleFactor;

            float glyphHeight = (float)(charBounds.W - charBounds.Y) / fontData.ScaleFactor;
            float glyphWidth = c == ' ' ? 0.5f : (UVs.Z - UVs.X) / (UVs.W - UVs.Y) * glyphHeight;

            var glyph = new UIQuad();
            glyph.position = new Vector2(XCursor + glyphWidth * 0.5f * Size, YCursor + glyphHeight * 0.5f * Size);
            glyph.size = new Vector2(glyphWidth * Size, glyphHeight * Size);
            Console.WriteLine($"Glyph size: {glyph.size}");
            glyph.UVOffset = new Vector2(UVs.X, 1 - UVs.Y);
            glyph.UVRange = new Vector2(UVs.Z - UVs.X, UVs.W - UVs.Y);
            glyph.colour = Colour;
            TextureManager.TryGetTexture(FontKey, UIScene.resolution, out var layer);
            glyph.textureLayer = layer;
            _glyphs.Add(glyph);
            XCursor += glyphWidth * Size;
        }
    }

    public void SubmitData(InstanceRenderer renderer)
    {
        if (!IsVisible) return;
        if (_isDirty)
        {
            UpdateGlyphs();
            _isDirty = false;
        }
        for (int i = 0; i < _glyphs.Count; i++)
        {
            renderer.AddInstance(_glyphs[i]);
        }
    }
}