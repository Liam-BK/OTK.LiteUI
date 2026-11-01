using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Rendering;
using OTK.LiteUI.UI.Utilities;

namespace OTK.LiteUI.UI.Components
{
    public enum TextAlignment
    {
        Left,
        Center,
        Right
    }

    public class Label : UIComponent, IRenderable, ILinkable
    {
        private Vector2 _origin = Vector2.Zero;

        [UIProperty]
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

        [UIProperty]
        public float Size
        {
            get => _size;
            set
            {
                _size = value;
                _isDirty = true;
            }
        }

        [UIProperty]
        public TextAlignment Alignment { get; set; } = TextAlignment.Left;

        public int GlyphCount
        {
            get
            {
                int result = 0;
                for (int i = 0; i < _lines.Count; i++) result += _lines[i].Count;
                return result;
            }
        }

        public int TotalLines
        {
            get => _lines.Count;
        }

        private readonly List<List<UIQuad>> _lines = [];

        public const float LineSpacing = 12.0f;

        public string FontKey
        {
            private get;
            set;
        }

        private string _text = "";

        [UIProperty]
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

        private Vector4 _bounds;

        [UIProperty]
        public override Vector4 Bounds
        {
            get
            {
                return _bounds;
            }
            set
            {
                _bounds = value;
                Size = (value.W - value.Y) * 0.5f;
                var y = value.Y + (value.W - value.Y) * 0.25f;
                float x;
                if (Alignment == TextAlignment.Left) x = _bounds.X;
                else if (Alignment == TextAlignment.Center) x = (_bounds.X + _bounds.Z) * 0.5f;
                else x = _bounds.Z;
                Origin = new Vector2(x, y);
                _isDirty = true;
            }
        }

        private string _pageLink = "";

        [UIProperty]
        public string PageLink
        {
            get
            {
                return _pageLink;
            }
            set
            {
                _pageLink = value;
            }
        }

        [UIProperty]
        public bool HideParentPageOnClick { get; set; } = true;

        private bool isClicked = false;

        public Label(UICanvas canvas, Vector2 origin, float size, string text = "", Vector4? colour = null)
        {
            this.Canvas = canvas;
            Origin = origin;
            Size = size;
            Colour = colour ?? Vector4.One;
            Text = text;
            FontKey = canvas.DefaultFontKey;
            float w = Origin.Y - (_lines.Count - 1) * (Size + LineSpacing) + Size;
            float y = w - _lines.Count * (Size + LineSpacing);
            _bounds = new Vector4(Origin.X, y, Origin.X, w);
            _lines.Add([]);
            canvas.Register(this);
            if (GetType() == typeof(Label)) Identifier = Canvas.GetNextIdentifier(typeof(Label));
        }

        public Label(Vector2 origin, float size, string text = "", Vector4? colour = null) : this(UICanvas.Default ?? throw new InvalidOperationException("Default Canvas must not be null for this Constructor. Try Creating a new UICanvas or assigning a UICanvas instance to UICanvas.Default"), origin, size, text, colour) { }

        public int FindInsertionOffset(int line)
        {
            int result = 0;
            for (int i = 0; i < line; i++)
            {
                result += _lines[i].Count;
            }
            return result + line;
        }

        public Vector2 FindCaretPosFromIndex(int caretIndex, int line)
        {
            float y = Origin.Y - line * (Size + LineSpacing) + Size * 0.5f;
            if (line < 0 || line >= _lines.Count) return new Vector2(Origin.X, y);
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
                var finalResult = new Vector2(MathF.Round((prevRight + currentLeft) * 0.5f, MidpointRounding.AwayFromZero), y);
                return finalResult;
            }
        }

        public int FindLineFromPos(MouseState mouse)
        {
            var convertedMouse = Canvas.ConvertMouseScreenCoords(mouse.Position);
            return FindLineFromPos(convertedMouse);
        }

        public int FindLineFromPos(Vector2 pos)
        {
            var visualOffset = Canvas.AnchorTypeOffsets[(int)Anchor](Canvas.CanvasSize, this);
            pos -= visualOffset;
            float lineHeight = Size + LineSpacing;
            float localY = Origin.Y + lineHeight - pos.Y;
            int line = (int)Math.Floor(localY / lineHeight);
            return Math.Clamp(line, 0, _lines.Count - 1);
        }

        public int FindCaretIndexFromPos(MouseState mouse)
        {
            var convertedMouse = Canvas.ConvertMouseScreenCoords(mouse.Position);
            return FindCaretIndexFromPos(convertedMouse);
        }

        public int FindCaretIndexFromPos(Vector2 pos)
        {
            if (!WithinBounds(pos)) return -1;
            int line = FindLineFromPos(pos);
            var visualOffset = Canvas.AnchorTypeOffsets[(int)Anchor](Canvas.CanvasSize, this);
            pos -= visualOffset;
            for (int i = 0; i < _lines[line].Count; i++)
            {
                if (_lines[line][i].position.X > pos.X) return i;
            }
            return _lines[line].Count;
        }

        public int FindLineEndIndex(int line)
        {
            return _lines[line].Count;
        }

        public void ForceUpdateGlyphs()
        {
            if (Canvas.DefaultFontKey == "") return;
            _lines.Clear();
            _lines.Add([]);
            _bounds = new Vector4(Origin.X, Origin.Y, Origin.X, Origin.Y);
            int currentLine = 0;
            float XCursor = Origin.X;
            float YCursor = Origin.Y;
            if (!Canvas.Fonts.TryGetValue(FontKey, out var fontData))
            {
                Console.WriteLine("No FontData");
                return;
            }
            for (int i = 0; i < Text.Length; i++)
            {
                char c = Text[i];
                if (c == '\n')
                {
                    YCursor -= Size + LineSpacing;
                    XCursor = Origin.X;
                    currentLine++;
                    _lines.Add([]);
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

                glyph.position = new Vector2(XCursor + glyphWidth * 0.5f * Size, YCursor - glyphHeight * 0.5f * Size - offset.Y * Size);
                glyph.size = new Vector2(glyphWidth * Size, glyphHeight * Size);

                float left = glyph.position.X - glyph.size.X * 0.5f;
                float right = glyph.position.X + glyph.size.X * 0.5f;

                _bounds.X = Math.Min(_bounds.X, left);
                _bounds.Z = Math.Max(_bounds.Z, right);

                glyph.UVOffset = new Vector2(UVs.X, 1 - UVs.W);
                glyph.UVRange = new Vector2(UVs.Z - UVs.X, UVs.W - UVs.Y);
                glyph.colour = Colour;
                Canvas.TryGetTexture(FontKey, out var layer);
                glyph.textureLayer = layer;
                _lines[currentLine].Add(glyph);
                XCursor += glyphWidth * Size + kern * Size + offset.X * Size;
            }
            _bounds.Y = Origin.Y - (_lines.Count - 1) * (Size + LineSpacing) - LineSpacing;
            _bounds.W = Origin.Y + Size;
            ShiftAlignment();
            _isDirty = false;
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
            if (_lines.Count <= 0 || line >= _lines.Count) return;
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

        public override bool OnClickDown(MouseState mouse)
        {
            if (!IsVisible || !WithinBounds(mouse)) return false;
            isClicked = true;
            return true;
        }

        public override bool OnClickUp(MouseState mouse)
        {
            if (!IsVisible || !WithinBounds(mouse))
            {
                isClicked = false;
                return false;
            }
            if (isClicked)
            {
                var page = Canvas.Get<Page>(PageLink);
                if (page is not null)
                {
                    page.IsVisible = true;
                    if (HideParentPageOnClick)
                    {
                        var parentPage = TopLevelPage;
                        if (parentPage is not null && parentPage != page) parentPage.IsVisible = false;
                    }
                }
                return true;
            }
            return false;
        }

        public void SubmitData(Renderer renderer)
        {
            if (!IsVisible || string.IsNullOrEmpty(Text) || Colour.W <= 0) return;
            if (_isDirty)
            {
                ForceUpdateGlyphs();
            }
            var visualOffset = Canvas.AnchorTypeOffsets[(int)Anchor](Canvas.CanvasSize, this);
            var clippingArea = ClipBounds;
            for (int i = 0; i < _lines.Count; i++)
            {
                for (int j = 0; j < _lines[i].Count; j++)
                {
                    var glyph = _lines[i][j];
                    float halfWidth = glyph.size.X * 0.5f;
                    float halfHeight = glyph.size.Y * 0.5f;
                    float left = glyph.position.X - halfWidth;
                    float bottom = glyph.position.Y - halfHeight;
                    float right = glyph.position.X + halfWidth;
                    float top = glyph.position.Y + halfHeight;
                    if (clippingArea is not null && !(right < clippingArea.Value.X || left > clippingArea.Value.Z || top < clippingArea.Value.Y || bottom > clippingArea.Value.W))
                    {
                        var temp = Utils.Clip(glyph, clippingArea.Value);
                        temp.position += visualOffset;
                        renderer.AddInstance(temp);
                    }
                    else if (clippingArea is null)
                    {
                        var temp = glyph;
                        temp.position += visualOffset;
                        renderer.AddInstance(temp);
                    }
                }
            }
        }
    }
}