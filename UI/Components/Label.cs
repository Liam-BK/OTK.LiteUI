using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

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

    public int GlyphCount
    {
        get
        {
            int result = 0;
            for (int i = 0; i < _lines.Count; i++) result += _lines[i].Count;
            return result;
        }
    }

    private List<List<UIQuad>> _lines = new List<List<UIQuad>>();

    public const float _lineSpacing = 12.0f;

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
            if (_lines.Count <= 0 || _lines[0].Count <= 0) return new Vector4(Origin.X, Origin.Y, Origin.X, Origin.Y);

            Vector4 result = new Vector4(_lines[0][0].position.X - _lines[0][0].size.X * 0.5f, _lines[0][0].position.Y - _lines[0][0].size.Y * 0.5f, _lines[0][0].position.X + _lines[0][0].size.X * 0.5f, _lines[0][0].position.Y + _lines[0][0].size.Y * 0.5f);
            foreach (var line in _lines)
            {
                foreach (var glyph in line)
                {
                    var pos = glyph.position;
                    var halfSize = glyph.size * 0.5f;
                    result.X = Math.Min(result.X, pos.X - halfSize.X);
                    result.Y = Math.Min(result.Y, pos.Y - halfSize.Y);
                    result.Z = Math.Max(result.Z, pos.X + halfSize.X);
                    result.W = Math.Max(result.W, pos.Y + halfSize.Y);
                }
            }
            return result;
        }
    }

    private int NumberOfLines
    {
        get
        {
            int count = 1;
            for (int i = 0; i < Text.Length; i++)
            {
                char c = Text[i];
                if (c == '\n')
                {
                    count++;
                }
            }
            return count;
        }
    }

    public Label(Vector2 origin, float size, string text = "", Vector4? colour = null)
    {
        UIScene.Register(this);
        Origin = origin;
        Size = size;
        Colour = colour ?? Vector4.One;
        Text = text;
        FontKey = FontManager.DefaultFontKey;
        _lines.Add(new List<UIQuad>());
    }

    public Vector2 FindCaretPosFromIndex(int caretIndex, int line)
    {
        float y = Origin.Y - line * (Size + _lineSpacing) + Size * 0.5f;
        int lineGlyphCount = _lines[line].Count;
        if (lineGlyphCount == 0) return new Vector2(Origin.X, y);
        else if (_lines[line].Count == 1)
        {
            var glyph = _lines[line][0];
            if (caretIndex <= 0) return new Vector2(glyph.position.X - glyph.size.X * 0.5f, y);
            else return new Vector2(glyph.position.X + glyph.size.X * 0.5f, y);
        }
        else
        {
            if (caretIndex <= 0)
            {
                var glyph = _lines[line][0];
                var result = new Vector2(glyph.position.X - glyph.size.X * 0.5f, y);
                return result;
            }
            else if (caretIndex >= lineGlyphCount)
            {
                var result = new Vector2(_lines[line][^1].position.X + _lines[line][^1].size.X * 0.5f, y);
                return result;
            }
            var currentGlyph = _lines[line][caretIndex];
            var previousGlyph = _lines[line][caretIndex - 1];
            float prevRight = previousGlyph.position.X + previousGlyph.size.X * 0.5f;
            float currentLeft = currentGlyph.position.X - currentGlyph.size.X * 0.5f;
            var finalResult = new Vector2((prevRight + currentLeft) * 0.5f, y);
            return finalResult;
        }
    }

    public int FindLineFromMousePos(MouseState mouse)
    {
        var convertedMouse = UIScene.ConvertMouseScreenCoords(mouse.Position);
        float lineHeight = Size + _lineSpacing;
        float localY = Origin.Y + lineHeight - convertedMouse.Y;
        int line = (int)Math.Floor(localY / lineHeight);
        return Math.Clamp(line, 0, _lines.Count - 1);
    }

    public int FindCaretIndexFromMousePos(MouseState mouse)
    {
        if (!WithinBounds(mouse)) return -1;
        var convertedMouse = UIScene.ConvertMouseScreenCoords(mouse.Position);
        int line = FindLineFromMousePos(mouse);
        for (int i = 0; i < _lines[line].Count; i++)
        {
            if (_lines[line][i].position.X > convertedMouse.X) return i;
        }
        return _lines[line].Count;
    }

    public int FindLineEndIndex(int line)
    {
        return _lines[line].Count;
    }

    private void UpdateGlyphs()
    {
        _lines.Clear();
        _lines.Add(new List<UIQuad>());
        int currentLine = 0;
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
                currentLine++;
                _lines.Add(new List<UIQuad>());
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
            _lines[currentLine].Add(glyph);
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
            ShiftAlignment(i);
            offset += line.Length;
        }
    }

    private void ShiftAlignment(int line)
    {
        if (_lines[line].Count == 0) return;
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
        var left = _lines[line][0].position.X - _lines[line][0].size.X * 0.5f;
        var right = _lines[line][^1].position.X + _lines[line][^1].size.X * 0.5f;
        var shiftAmount = (right - left) * multiplier;
        for (int i = 0; i < _lines[line].Count; i++)
        {
            var glyph = _lines[line][i];
            var x = glyph.position.X;
            var y = glyph.position.Y;
            glyph.position = new Vector2(x - shiftAmount, y);
            _lines[line][i] = glyph;
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
        for (int i = 0; i < _lines.Count; i++)
        {
            for (int j = 0; j < _lines[i].Count; j++)
            {
                renderer.AddInstance(_lines[i][j]);
            }
        }
    }
}