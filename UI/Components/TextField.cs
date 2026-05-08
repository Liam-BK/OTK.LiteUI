using System.Diagnostics;
using System.Text;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public enum TextFieldMode
{
    SingleLine,
    MultiLine
}

public class TextField : NineSlice
{
    private readonly Label label;

    public string Text
    {
        get => label.Text ?? "";
        set
        {
            if (label is not null) label.Text = value;
        }
    }

    private UIQuad caret = new();

    public TextFieldMode Mode = TextFieldMode.MultiLine;

    private int GlobalCaretIndex
    {
        get
        {
            if (label is null) return 0;
            return label.FindInsertionOffset(caretLine) + caretIndex;
        }
    }

    private Vector4 ViewPort
    {
        get
        {
            return new Vector4(Bounds.X + Inset, Bounds.Y + Inset, Bounds.Z - Inset, Bounds.W - Inset);
        }
    }

    public override Vector4 Bounds
    {
        get => base.Bounds;
        set
        {
            base.Bounds = value;
            if (label is not null)
            {
                label.ClipBounds = ViewPort;
                UpdateLabelOrigin();
            }
        }
    }

    private Vector2 LabelPosition
    {
        get
        {
            return new Vector2(Bounds.X + Inset - ScrollOffset.X, Bounds.W - TextSize - Inset + ScrollOffset.Y);
        }
    }

    private const float defaultTextSize = 25.0f;

    private float _textSize;

    public float TextSize
    {
        private get
        {
            return _textSize;
        }
        set
        {
            _textSize = value;
        }
    }

    public override bool CanFocus => true;

    public Vector2 MaxScroll
    {
        get
        {
            if (label is null) return Vector2.Zero;
            var view = ViewPort;
            return new Vector2(Math.Max(label.Width - (view.Z - view.X), 0), Math.Max(label.Height - (view.W - view.Y), 0));
        }
    }

    private Vector2 _scrollOffset = Vector2.Zero;

    public Vector2 ScrollOffset
    {
        get
        {
            return _scrollOffset;
        }
        set
        {
            _scrollOffset = new Vector2(Math.Clamp(value.X, 0, MaxScroll.X), Math.Clamp(value.Y, 0, MaxScroll.Y));
        }
    }

    private const float caretBlinkTime = 0.5f;
    private static readonly Stopwatch stopwatch = new();
    private bool _caretVisible = false;
    private bool CaretVisible
    {
        get
        {
            float halfWidth = caret.size.X * 0.5f;
            float halfHeight = caret.size.Y * 0.5f;
            var view = ViewPort;
            return _caretVisible && CanFocus && IsFocused && !(caret.position.X - halfWidth > view.Z || caret.position.X + halfWidth < view.X || caret.position.Y - halfHeight > view.W || caret.position.Y + halfHeight < view.Y);
        }
    }

    public int caretIndex = 0;
    public int caretLine = 0;
    private int desiredColumn = 0;

    public TextField(Vector4 bounds, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(bounds, inset, uvInset, colour)
    {
        TextSize = Math.Min(Height * 0.5f, defaultTextSize);
        var textColour = new Vector4(0, 0, 0, 1);
        label = new Label(LabelPosition, TextSize)
        {
            Colour = textColour,
            ClipBounds = ViewPort
        };
        caret.size = new Vector2(UIScene.InvDPIScaleX, TextSize + Label.LineSpacing);
        caret.colour = textColour;
        UpdateCaretPosition();
        stopwatch.Start();
    }

    private void UpdateLabelOrigin()
    {
        label.Origin = LabelPosition;
        label.ForceUpdateGlyphs();
        UpdateCaretPosition();

    }

    private void ApplyAutoScroll()
    {
        var view = ViewPort;

        float halfWidth = caret.size.X * 0.5f;
        float halfHeight = caret.size.Y * 0.5f;

        float left = caret.position.X - halfWidth;
        float right = caret.position.X + halfWidth;
        float bottom = caret.position.Y - halfHeight;
        float top = caret.position.Y + halfHeight;

        Vector2 scroll = ScrollOffset;

        if (right > view.Z)
            scroll.X += right - view.Z;

        else if (left < view.X)
            scroll.X -= view.X - left;

        if (top > view.W)
        {
            float diff = top - view.W;
            scroll.Y -= diff;
        }

        else if (bottom < view.Y)
        {
            float diff = view.Y - bottom;
            scroll.Y += diff;
        }

        ScrollOffset = scroll;

        UpdateLabelOrigin();
    }

    private void RemoveCharacter()
    {
        if (caretIndex >= label.FindLineEndIndex(caretLine)) return;
        var sb = new StringBuilder(Text);
        sb.Remove(GlobalCaretIndex, 1);
        Text = sb.ToString();
        label.ForceUpdateGlyphs();
    }

    private void UpdateCaretPosition()
    {
        caret.position = label.FindCaretPosFromIndex(caretIndex, caretLine);
    }

    private void UpdateBlinkTime()
    {
        if (stopwatch.ElapsedMilliseconds >= caretBlinkTime * 1000.0f)
        {
            stopwatch.Restart();
            _caretVisible = !_caretVisible;
        }
    }

    private void SetCaretVisible()
    {
        _caretVisible = true;
        stopwatch.Restart();
    }

    public override void OnTextInput(TextInputEventArgs e)
    {
        if (!CanFocus || !IsFocused || !IsVisible) return;
        base.OnTextInput(e);
        var sb = new StringBuilder(Text);
        var character = (char)e.Unicode;
        sb.Insert(GlobalCaretIndex, character);
        caretIndex++;
        desiredColumn = caretIndex;
        Text = sb.ToString();
        label.ForceUpdateGlyphs();
        UpdateCaretPosition();
        ApplyAutoScroll();
        SetCaretVisible();
    }

    public override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Keys.Enter)
        {
            if (Mode == TextFieldMode.MultiLine)
            {
                var sb = new StringBuilder(Text);
                sb.Insert(GlobalCaretIndex, '\n');
                caretIndex = 0;
                desiredColumn = caretIndex;
                caretLine++;
                Text = sb.ToString();
                label.ForceUpdateGlyphs();
            }
            else if (Mode == TextFieldMode.SingleLine)
            {
                UIScene.FocusedComponent = null;
                return;
            }
        }
        else if (e.Key == Keys.Backspace)
        {
            if (caretIndex == 0)
            {
                caretLine = Math.Max(0, caretLine - 1);
                caretIndex = label.FindLineEndIndex(caretLine);
            }
            else
            {
                caretIndex = Math.Max(0, caretIndex - 1);
            }
            desiredColumn = caretIndex;
            RemoveCharacter();
        }
        else if (e.Key == Keys.Delete)
        {
            RemoveCharacter();
        }
        else if (e.Key == Keys.Up)
        {
            caretLine = Math.Max(0, caretLine - 1);
            caretIndex = Math.Min(label.FindLineEndIndex(caretLine), desiredColumn);
        }
        else if (e.Key == Keys.Down)
        {
            caretLine = Math.Min(label.TotalLines - 1, caretLine + 1);
            caretIndex = Math.Min(label.FindLineEndIndex(caretLine), desiredColumn);
        }
        else if (e.Key == Keys.Left)
        {
            if (caretIndex <= 0 && caretLine > label.TotalLines - 1)
            {
                caretLine = Math.Max(0, caretLine - 1);
                caretIndex = label.FindLineEndIndex(caretLine);
            }
            else
            {
                caretIndex = Math.Max(0, caretIndex - 1);
            }
            desiredColumn = caretIndex;
        }
        else if (e.Key == Keys.Right)
        {
            if (caretLine < label.TotalLines - 1 && caretIndex >= label.FindLineEndIndex(caretLine))
            {
                caretIndex = 0;
                caretLine++;
            }
            else if (caretIndex < label.FindLineEndIndex(caretLine))
            {
                caretIndex++;
            }
            desiredColumn = caretIndex;
        }
        UpdateCaretPosition();
        ApplyAutoScroll();
        SetCaretVisible();
    }

    public override bool OnMouseWheel(MouseState mouse)
    {
        if (!WithinBounds(mouse) || !IsVisible) return base.OnMouseWheel(mouse);

        ScrollOffset = new Vector2(
            ScrollOffset.X - mouse.ScrollDelta.X * ScrollBar.scrollSensitivity,
            ScrollOffset.Y - mouse.ScrollDelta.Y * ScrollBar.scrollSensitivity);

        UpdateLabelOrigin();
        return true;
    }

    public override bool OnClickDown(MouseState mouse)
    {
        if (!WithinBounds(mouse) || !IsVisible)
        {
            UIScene.FocusedComponent = null;
            return false;
        }
        Vector2 convertedMouse = UIScene.ConvertMouseScreenCoords(mouse.Position);
        UIScene.FocusedComponent = this;
        caretLine = label.FindLineFromPos(mouse);
        caretIndex = convertedMouse.X <= label.Bounds.Z ? label.FindCaretIndexFromPos(mouse) : label.FindLineEndIndex(caretLine);
        desiredColumn = caretIndex;
        UpdateCaretPosition();

        return base.OnClickDown(mouse);
    }

    public override bool OnMouseMove(MouseState mouse)
    {
        return base.OnMouseMove(mouse);
    }

    public override void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
    {
        base.OnUpdate(deltaTime, mouse, keyboard);
        UpdateBlinkTime();
    }

    public override void SubmitData(InstanceRenderer renderer)
    {
        base.SubmitData(renderer);
        if (CaretVisible) renderer.AddInstance(Utils.Clip(caret, ViewPort));
    }
}