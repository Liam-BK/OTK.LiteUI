using System.Diagnostics;
using System.Text;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

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

    private StringBuilder sb = new StringBuilder();

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

    UIQuad caret = new UIQuad();

    private float caretBlinkTime = 0.5f;

    private static Stopwatch timer = new Stopwatch();

    public override bool CanFocus => true;

    private int caretIndex = 0;
    private int caretLine = 0;

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

    public TextField(Vector4 bounds, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(bounds, inset, uvInset, colour)
    {
        label = new Label(new Vector2(Bounds.X + Inset, Center.Y - Height * 0.25f), Height * 0.5f, colour: new Vector4(0, 0, 0, 1));
        caret.position = label.FindCaretPosFromIndex(0, 0);
        caret.size = new Vector2(2, label.Size + Label._lineSpacing * 0.5f);
        caret.textureLayer = -1;
        caret.colour = new Vector4(0, 0, 0, 1);
        timer.Start();
    }

    private void UpdateCaretPos()
    {
        caret.position = label.FindCaretPosFromIndex(caretIndex, caretLine);
        caretVisible = true;
        timer.Restart();
    }

    private void RemoveCharacter()
    {
        var sb = new StringBuilder();
        sb.Append(Text);
        sb.Remove(caretIndex, 1);
        Text = sb.ToString();
        label.ForceUpdateGlyphs();
    }

    public override bool OnClickDown(MouseState mouse)
    {
        if (!WithinBounds(mouse))
        {
            UIScene.FocusedComponent = null;
            return false;
        }
        Vector2 convertedMouse = UIScene.ConvertMouseScreenCoords(mouse.Position);
        UIScene.FocusedComponent = this;
        caretLine = label.FindLineFromMousePos(mouse);
        caretIndex = convertedMouse.X <= label.Bounds.Z ? label.FindCaretIndexFromMousePos(mouse) : label.FindLineEndIndex(caretLine);
        UpdateCaretPos();


        return base.OnClickDown(mouse);
    }

    public override void OnTextInput(TextInputEventArgs e)
    {
        if (!CanFocus || !IsFocused || !IsVisible) return;
        base.OnTextInput(e);
        var sb = new StringBuilder();
        sb.Append(Text);
        var character = (char)e.Unicode;
        if (caretIndex >= Text.Length)
        {
            sb.Append(character);
        }
        else
        {
            sb.Insert(caretIndex, character);
        }
        caretIndex++;
        Text = sb.ToString();
        label.ForceUpdateGlyphs();
        UpdateCaretPos();
    }

    public override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Key == Keys.Left) caretIndex = Math.Max(0, caretIndex - 1);
        if (e.Key == Keys.Right) caretIndex = Math.Min(Text.Length, caretIndex + 1);
        if (e.Key == Keys.Up) caretIndex = 0;
        if (e.Key == Keys.Down) caretIndex = Text.Length;
        if (e.Key == Keys.Backspace)
        {
            if (caretIndex <= 0) return;
            caretIndex--;
            RemoveCharacter();
        }
        if (e.Key == Keys.Delete)
        {
            if (caretIndex >= Text.Length) return;
            RemoveCharacter();
        }
        UpdateCaretPos();
    }

    public override void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
    {
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
        if (caretVisible && CanFocus && IsFocused) renderer.AddInstance(caret);
    }
}