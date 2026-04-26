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
    private const float _lineSpacing = 12.0f;
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

    public override Vector4 Bounds
    {
        get
        {
            if (_glyphs.Count <= 0) return new Vector4(Origin.X, Origin.Y, Origin.X, Origin.Y);

            Vector4 result = new Vector4(_glyphs[0].position.X - _glyphs[0].size.X * 0.5f, _glyphs[0].position.Y - _glyphs[0].size.Y * 0.5f, _glyphs[0].position.X + _glyphs[0].size.X * 0.5f, _glyphs[0].position.Y + _glyphs[0].size.Y * 0.5f);
            foreach (var glyph in _glyphs)
            {
                var pos = glyph.position;
                var halfSize = glyph.size * 0.5f;
                result.X = Math.Min(result.X, pos.X - halfSize.X);
                result.Y = Math.Min(result.Y, pos.Y - halfSize.Y);
                result.Z = Math.Max(result.Z, pos.X + halfSize.X);
                result.W = Math.Max(result.W, pos.Y + halfSize.Y);
            }
            return result;
        }
    }

    public Label(Vector2 origin, float size, string text = "", Vector4? colour = null)
    {
        Origin = origin;
        Size = size;
        Colour = colour ?? Vector4.One;
        Text = text;
        FontKey = FontManager.DefaultFontKey;
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
                if (kern > 0) Console.WriteLine($"{c} : {Text[i + 1]} kern: {kern}");
            }
            offset /= fontData.ScaleFactor;

            float glyphHeight = (float)(charBounds.W - charBounds.Y) / fontData.ScaleFactor;
            float glyphWidth = c == ' ' ? 0.5f : (UVs.Z - UVs.X) / (UVs.W - UVs.Y) * glyphHeight;

            var glyph = new UIQuad();
            glyph.position = new Vector2(XCursor + glyphWidth * 0.5f * Size, YCursor - glyphHeight * 0.5f * Size - offset.Y * Size);
            glyph.size = new Vector2(glyphWidth * Size, glyphHeight * Size);
            glyph.UVOffset = new Vector2(UVs.X, 1 - UVs.W);
            glyph.UVRange = new Vector2(UVs.Z - UVs.X, UVs.W - UVs.Y);
            glyph.colour = Colour;
            TextureManager.TryGetTexture(FontKey, UIScene.resolution, out var layer);
            glyph.textureLayer = layer;
            _glyphs.Add(glyph);
            XCursor += glyphWidth * Size + kern * Size;
            XCursor += offset.X * Size;
        }
        ShiftAlignment();
    }

    private void ShiftAlignment()
    {
        var lines = _text.Split('\n');
        var offset = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            ShiftLineAlignment(offset, offset + line.Length - 1);
            offset += line.Length;
        }
    }

    private void ShiftLineAlignment(int startIndex, int endIndex)
    {
        float multiplier;
        switch (Alignment)
        {
            case TextAlignment.Center:
                multiplier = 0.5f;
                break;
            case TextAlignment.Right:
                multiplier = 1;
                break;
            default:
                return;
        }
        var left = _glyphs[startIndex].position.X - _glyphs[startIndex].size.X * 0.5f;
        var right = _glyphs[endIndex].position.X + _glyphs[endIndex].size.X * 0.5f;
        var shiftAmount = (right - left) * multiplier;
        for (int i = startIndex; i <= endIndex; i++)
        {
            var glyph = _glyphs[i];
            var x = glyph.position.X;
            var y = glyph.position.Y;
            glyph.position = new Vector2(x - shiftAmount, y);
            _glyphs[i] = glyph;
        }
    }

    public void SubmitData(InstanceRenderer renderer)
    {
        if (!IsVisible || string.IsNullOrEmpty(Text)) return;
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