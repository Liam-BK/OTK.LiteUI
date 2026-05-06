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
    private Label label;

    public string Text
    {
        get
        {
            if (label is null) return "";
            return label.Text;
        }
        set
        {
            if (label is not null) label.Text = value;
        }
    }

    public TextFieldMode Mode = TextFieldMode.MultiLine;

    public Vector4 TextColour
    {
        set
        {
            if (label is not null) label.Colour = value;
        }
    }

    public string Font
    {
        set
        {
            if (label is not null) label.FontKey = value;
        }
    }

    private bool caretVisible = false;

    private int globalCaretIndex
    {
        get
        {
            if (label is null) return 0;
            return label.FindInsertionOffset(caretLine) + caretIndex;
        }
    }

    UIQuad caret = new UIQuad();

    private float caretBlinkTime = 0.5f;

    private static Stopwatch timer = new Stopwatch();

    public override bool CanFocus => true;
    public Vector2 Scroll
    {
        get;
        private set;
    } = Vector2.Zero;

    private Vector2 ScrollLimits
    {
        get
        {
            if (label is null) return Vector2.Zero;
            return new Vector2(Math.Max(label.Width - (Width - Inset * 2), 0), Math.Max(label.Height - (Height - Inset * 2), 0));
        }
    }

    public int caretIndex = 0;
    public int caretLine = 0;
    private int desiredColumn = 0;

    public override bool IsVisible
    {
        get
        {
            return base.IsVisible;
        }
        set
        {
            base.IsVisible = value;
            label.IsVisible = value;
        }
    }

    public float TextSize
    {
        set
        {
            label.Size = value;
        }
    }

    public override Vector4 Bounds
    {
        get
        {
            return base.Bounds;
        }
        set
        {
            base.Bounds = value;
            if (label is not null) label.ClipBounds = new Vector4(Bounds.X + Inset, Bounds.Y + Inset, Bounds.Z - Inset, Bounds.W - Inset);
        }
    }

    private const float defaultTextSize = 25.0f;

    private readonly Vector2 labelOrigin;

    public TextField(Vector4 bounds, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(bounds, inset, uvInset, colour)
    {
        float textSize = Math.Min(Height * 0.5f, defaultTextSize);
        labelOrigin = new Vector2(Bounds.X + Inset, Bounds.W - textSize - Inset);
        label = new Label(labelOrigin, textSize, colour: new Vector4(0, 0, 0, 1));
        caret.position = label.FindCaretPosFromIndex(0, 0);
        caret.size = new Vector2(2, label.Size + Label._lineSpacing * 0.5f);
        caret.textureLayer = -1;
        caret.colour = new Vector4(0, 0, 0, 1);
        timer.Start();
        label.ClipBounds = new Vector4(Bounds.X + Inset, Bounds.Y + Inset, Bounds.Z - Inset, Bounds.W - Inset);
    }

    private void UpdateCaretPos()
    {
        caret.position = label.FindCaretPosFromIndex(caretIndex, caretLine);
        caretVisible = true;
        timer.Restart();
    }

    private void RemoveCharacter()
    {
        var sb = new StringBuilder(Text);
        sb.Remove(globalCaretIndex, 1);
        Text = sb.ToString();
        label.ForceUpdateGlyphs();
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
        UpdateCaretPos();


        return base.OnClickDown(mouse);
    }

    public override void OnTextInput(TextInputEventArgs e)
    {
        if (!CanFocus || !IsFocused || !IsVisible) return;
        base.OnTextInput(e);
        var sb = new StringBuilder(Text);
        var character = (char)e.Unicode;
        sb.Insert(globalCaretIndex, character);
        caretIndex++;
        desiredColumn = caretIndex;
        Text = sb.ToString();
        label.ForceUpdateGlyphs();
        UpdateCaretPos();
    }

    public override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        if (!IsVisible || !CanFocus || !IsFocused) return;
        base.OnKeyDown(e);
        if (e.Key == Keys.Left)
        {
            if (caretIndex == 0 && caretLine > 0)
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
        if (e.Key == Keys.Right)
        {
            int endIndex = label.FindLineEndIndex(caretLine);
            if (caretIndex == endIndex && caretLine < label.TotalLines - 1)
            {
                caretLine++;
                caretIndex = 0;
            }
            else
            {
                caretIndex = Math.Min(endIndex, caretIndex + 1);
            }
            desiredColumn = caretIndex;
        }
        if (e.Key == Keys.Up)
        {
            if (caretLine <= 0)
            {
                caretIndex = 0;
                desiredColumn = caretIndex;
            }
            else
            {
                caretLine--;
                caretIndex = Math.Min(desiredColumn, label.FindLineEndIndex(caretLine));
            }
        }
        if (e.Key == Keys.Down)
        {
            if (caretLine >= label.TotalLines - 1)
            {
                caretIndex = label.FindLineEndIndex(caretLine);
                desiredColumn = caretIndex;
            }
            else
            {
                caretLine++;
                caretIndex = Math.Min(desiredColumn, label.FindLineEndIndex(caretLine));
            }
        }
        if (e.Key == Keys.Backspace)
        {
            if (caretIndex == 0 && caretLine <= 0) return;
            else if (caretIndex == 0 && caretLine > 0)
            {
                caretLine--;
                caretIndex = label.FindLineEndIndex(caretLine);
            }
            else
            {
                caretIndex--;
            }
            desiredColumn = caretIndex;
            RemoveCharacter();
        }
        if (e.Key == Keys.Delete)
        {
            if (caretIndex >= label.FindLineEndIndex(caretLine) && caretLine >= label.TotalLines - 1) return;
            RemoveCharacter();
        }
        if (e.Key == Keys.Enter && Mode == TextFieldMode.MultiLine)
        {
            var sb = new StringBuilder(Text);
            sb.Insert(globalCaretIndex, '\n');
            caretIndex = 0;
            desiredColumn = caretIndex;
            caretLine++;
            Text = sb.ToString();
            label.ForceUpdateGlyphs();
        }
        if (e.Key == Keys.Enter && Mode == TextFieldMode.SingleLine)
        {
            UIScene.FocusedComponent = null;
        }
        UpdateCaretPos();
    }

    public override bool OnMouseWheel(MouseState mouse)
    {
        if (!WithinBounds(mouse) || !IsVisible) return base.OnMouseWheel(mouse);

        Scroll = new Vector2(
            Math.Clamp(Scroll.X - mouse.ScrollDelta.X * ScrollBar.scrollSensitivity, 0, ScrollLimits.X),
            Math.Clamp(Scroll.Y - mouse.ScrollDelta.Y * ScrollBar.scrollSensitivity, 0, ScrollLimits.Y)
        );

        label.Origin = new Vector2(
            labelOrigin.X - Scroll.X,
            labelOrigin.Y + Scroll.Y
        );

        UpdateCaretPos();
        return true;
    }

    public override void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
    {
        if (!IsVisible) return;
        base.OnUpdate(deltaTime, mouse, keyboard);
        if (timer.ElapsedMilliseconds >= caretBlinkTime * 1000)
        {
            caretVisible = !caretVisible;
            timer.Restart();
        }
    }

    public override void SubmitData(InstanceRenderer renderer)
    {
        if (!IsVisible) return;
        base.SubmitData(renderer);
        if (caretVisible && CanFocus && IsFocused) renderer.AddInstance(Utils.Clip(caret, label.ClipBounds));
    }
}