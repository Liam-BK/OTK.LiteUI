using System.Runtime.InteropServices;
using System.Text;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;

public enum TextFieldMode
{
    SingleLine,
    MultiLine
}

public class TextField : NineSlice
{
    protected readonly Label label;

    public string Text
    {
        get => label.Text ?? "";
        set
        {
            if (label is not null) label.Text = value;
        }
    }

    private UIQuad caret = new();

    protected bool LockMode = false;

    private TextFieldMode _mode = TextFieldMode.MultiLine;

    public TextFieldMode Mode
    {
        get
        {
            return _mode;
        }
        set
        {
            if (!LockMode) _mode = value;
        }
    }

    private int GlobalCaretIndex
    {
        get
        {
            if (label is null) return 0;
            return label.FindInsertionOffset(caretLine) + caretIndex;
        }
    }

    private int GlobalAnchorIndex
    {
        get
        {
            if (label is null) return 0;
            return label.FindInsertionOffset(selectionAnchorLine) + selectionAnchorIndex;
        }
    }

    private int SubstringStartIndex
    {
        get => GlobalCaretIndex <= GlobalAnchorIndex ? GlobalCaretIndex : GlobalAnchorIndex;
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
            _scrollOffset = new Vector2(Math.Clamp(value.X, 0, MaxScroll.X), Mode == TextFieldMode.SingleLine ? 0 : Math.Clamp(value.Y, 0, MaxScroll.Y));
        }
    }

    private const float caretBlinkTime = 0.5f;

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

    private float _caretBlinkAccumulation = 0;

    protected int caretIndex = 0;
    protected int caretLine = 0;
    private int desiredColumn = 0;

    protected int selectionAnchorIndex = 0;
    protected int selectionAnchorLine = 0;

    private bool IsSelecting
    {
        get => GlobalAnchorIndex != GlobalCaretIndex;
    }

    private bool IsClicked = false;

    private int SelectedRange
    {
        get => Math.Abs(GlobalAnchorIndex - GlobalCaretIndex);
    }

    public event Action? OnEnterPressed;

    public TextField(Vector4 bounds, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(bounds, inset, uvInset, colour)
    {
        TextSize = Math.Min(Math.Max(1, Height * 0.5f), defaultTextSize);
        var textColour = new Vector4(0, 0, 0, 1);
        label = new Label(LabelPosition, TextSize)
        {
            Colour = textColour,
            ClipBounds = ViewPort
        };
        caret.size = new Vector2(UIScene.InvDPIScaleX, TextSize + Label.LineSpacing);
        caret.colour = textColour;
        UpdateCaretPosition();
        UIScene.Deregister(label);
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

    private void RemoveCharacter(int range = 1)
    {
        if (GlobalCaretIndex >= Text.Length) return;
        var sb = new StringBuilder(Text);
        sb.Remove(SubstringStartIndex, range);
        Text = sb.ToString();
        label.ForceUpdateGlyphs();
    }

    private void UpdateCaretPosition()
    {
        caret.position = label.FindCaretPosFromIndex(caretIndex, caretLine);
    }

    private void UpdateBlinkTime(float deltaTime)
    {
        _caretBlinkAccumulation += deltaTime;
        if (_caretBlinkAccumulation >= caretBlinkTime)
        {
            _caretBlinkAccumulation -= caretBlinkTime;
            _caretVisible = !_caretVisible;
        }
    }

    private void SetCaretVisible()
    {
        _caretVisible = true;
        _caretBlinkAccumulation = 0;
    }

    private void MoveLeft()
    {
        if (caretIndex <= 0 && caretLine > 0)
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

    private void MoveUp()
    {
        if (caretLine == 0)
        {
            caretIndex = 0;
        }
        else
        {
            caretLine = Math.Max(0, caretLine - 1);
            caretIndex = Math.Min(label.FindLineEndIndex(caretLine), desiredColumn);
        }
    }

    private void MoveRight()
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

    private void MoveDown()
    {
        if (caretLine >= label.TotalLines - 1)
        {
            caretIndex = label.FindLineEndIndex(caretLine);
        }
        else
        {
            caretLine = Math.Min(label.TotalLines - 1, caretLine + 1);
            caretIndex = Math.Min(label.FindLineEndIndex(caretLine), desiredColumn);
        }
    }

    private void CollapseSelectionLeft()
    {
        var globalIndex = GlobalCaretIndex;
        var globalAnchor = GlobalAnchorIndex;
        if (!IsSelecting) return;
        if (globalAnchor > globalIndex)
        {
            selectionAnchorIndex = caretIndex;
            selectionAnchorLine = caretLine;
        }
        else if (globalIndex > globalAnchor)
        {
            caretIndex = selectionAnchorIndex;
            caretLine = selectionAnchorLine;
        }
    }

    private void CollapseSelectionRight()
    {
        var globalIndex = GlobalCaretIndex;
        var globalAnchor = GlobalAnchorIndex;
        if (!IsSelecting) return;
        if (globalAnchor > globalIndex)
        {
            caretIndex = selectionAnchorIndex;
            caretLine = selectionAnchorLine;
        }
        else if (globalIndex > globalAnchor)
        {
            selectionAnchorIndex = caretIndex;
            selectionAnchorLine = caretLine;
        }
    }

    private void HandleTextEditShortcuts(KeyboardKeyEventArgs e)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            if (e.Control && e.Key == Keys.C) Copy();
            else if (e.Control && e.Key == Keys.X) Cut();
            else if (e.Control && e.Key == Keys.V) Paste();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            if (e.Command && e.Key == Keys.C) Copy();
            else if (e.Command && e.Key == Keys.X) Cut();
            else if (e.Command && e.Key == Keys.V) Paste();
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            if (e.Shift && e.Control && e.Key == Keys.C) Copy();
            else if (e.Shift && e.Control && e.Key == Keys.X) Cut();
            else if (e.Shift && e.Control && e.Key == Keys.V) Paste();
        }
    }

    private void Copy()
    {
        UIScene.ClipboardString = Text.Substring(SubstringStartIndex, Math.Max(0, SelectedRange));
    }

    private void Cut()
    {
        Copy();
        RemoveCharacter(SelectedRange);
        CollapseSelectionLeft();
    }

    private void Paste()
    {
        var inserted = UIScene.ClipboardString;
        int caretShift = inserted.Length;
        RemoveCharacter(SelectedRange);
        CollapseSelectionLeft();
        var sb = new StringBuilder(Text);
        sb.Insert(SubstringStartIndex, inserted);
        Text = sb.ToString();
        label.ForceUpdateGlyphs();
        caretIndex += caretShift;
        CollapseSelectionRight();
    }

    private void SubmitHighlightData(InstanceRenderer renderer)
    {
        var globalIndex = GlobalCaretIndex;
        var globalAnchor = GlobalAnchorIndex;
        if (globalIndex == globalAnchor) return;
        if (caretLine == selectionAnchorLine)
        {
            var left = label.FindCaretPosFromIndex(globalIndex < globalAnchor ? caretIndex : selectionAnchorIndex, globalIndex < globalAnchor ? caretLine : selectionAnchorLine);
            var right = label.FindCaretPosFromIndex(globalIndex < globalAnchor ? selectionAnchorIndex : caretIndex, globalIndex < globalAnchor ? selectionAnchorLine : caretLine);
            var highlightQuad = new UIQuad
            {
                position = new Vector2((left.X + right.X) * 0.5f, (left.Y + right.Y) * 0.5f),
                size = new Vector2(right.X - left.X, TextSize + Label.LineSpacing),
                colour = new Vector4(0, 0, 1, 0.25f),
                textureLayer = -1
            };
            renderer.AddInstance(Utils.Clip(highlightQuad, ViewPort));
        }
        else
        {
            var startIndex = globalIndex < globalAnchor ? caretIndex : selectionAnchorIndex;
            var startLine = globalIndex < globalAnchor ? caretLine : selectionAnchorLine;
            var endIndex = globalIndex < globalAnchor ? selectionAnchorIndex : caretIndex;
            var endLine = globalIndex < globalAnchor ? selectionAnchorLine : caretLine;
            for (int i = startLine; i <= endLine; i++)
            {
                if (i == startLine)
                {
                    var left = label.FindCaretPosFromIndex(startIndex, i);
                    var right = label.FindCaretPosFromIndex(label.FindLineEndIndex(i), i);
                    var result = new UIQuad
                    {
                        position = new Vector2((left.X + right.X) * 0.5f, (left.Y + right.Y) * 0.5f),
                        size = new Vector2(right.X - left.X, TextSize + Label.LineSpacing),
                        colour = new Vector4(0, 0, 1, 0.25f),
                        textureLayer = -1
                    };
                    renderer.AddInstance(result);
                }
                else if (i == endLine)
                {
                    var left = label.FindCaretPosFromIndex(0, i);
                    var right = label.FindCaretPosFromIndex(endIndex, i);
                    var result = new UIQuad
                    {
                        position = new Vector2((left.X + right.X) * 0.5f, (left.Y + right.Y) * 0.5f),
                        size = new Vector2(right.X - left.X, TextSize + Label.LineSpacing),
                        colour = new Vector4(0, 0, 1, 0.25f),
                        textureLayer = -1
                    };
                    renderer.AddInstance(result);
                }
                else
                {
                    renderer.AddInstance(FullLineHighlight(i));
                }
            }
        }
    }

    private UIQuad FullLineHighlight(int line)
    {
        var left = label.FindCaretPosFromIndex(0, line);
        var right = label.FindCaretPosFromIndex(label.FindLineEndIndex(line), line);
        var result = new UIQuad
        {
            position = new Vector2((left.X + right.X) * 0.5f, (left.Y + right.Y) * 0.5f),
            size = new Vector2(right.X - left.X, TextSize + Label.LineSpacing),
            colour = new Vector4(0, 0, 1, 0.25f),
            textureLayer = -1
        };
        return result;
    }

    public override void OnTextInput(TextInputEventArgs e)
    {
        if (!CanFocus || !IsFocused || !IsVisible) return;
        base.OnTextInput(e);
        var character = (char)e.Unicode;
        if (IsSelecting)
        {
            RemoveCharacter(SelectedRange);
            CollapseSelectionLeft();
        }
        var sb = new StringBuilder(Text);
        sb.Insert(SubstringStartIndex, character);
        caretIndex++;
        CollapseSelectionRight();
        desiredColumn = caretIndex;
        Text = sb.ToString();
        label.ForceUpdateGlyphs();
        UpdateCaretPosition();
        ApplyAutoScroll();
        SetCaretVisible();
    }

    public override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        if (!CanFocus || !IsFocused || !IsVisible) return;
        base.OnKeyDown(e);
        if (e.Key == Keys.Enter)
        {
            if (Mode == TextFieldMode.MultiLine)
            {
                if (IsSelecting)
                {
                    RemoveCharacter(SelectedRange);
                    CollapseSelectionLeft();
                }
                var sb = new StringBuilder(Text);
                sb.Insert(GlobalCaretIndex, '\n');
                caretIndex = 0;
                desiredColumn = caretIndex;
                caretLine++;
                CollapseSelectionRight();
                Text = sb.ToString();
                label.ForceUpdateGlyphs();
            }
            else if (Mode == TextFieldMode.SingleLine)
            {
                UIScene.FocusedComponent = null;
                OnEnterPressed?.Invoke();
                return;
            }
        }
        else if (e.Key == Keys.Backspace)
        {
            bool canDelete = IsSelecting || caretIndex > 0 || caretLine > 0;
            if (!IsSelecting || e.Shift) MoveLeft();
            var range = SelectedRange;
            CollapseSelectionLeft();
            if (canDelete)
            {
                RemoveCharacter(Math.Max(1, range));
            }
        }
        else if (e.Key == Keys.Delete)
        {
            RemoveCharacter(Math.Max(1, SelectedRange));
            CollapseSelectionLeft();
        }
        else if (e.Key == Keys.Up)
        {
            if (!IsSelecting || e.Shift) MoveUp();
            if (!e.Shift) CollapseSelectionLeft();
        }
        else if (e.Key == Keys.Down)
        {
            if (!IsSelecting || e.Shift) MoveDown();
            if (!e.Shift) CollapseSelectionRight();
        }
        else if (e.Key == Keys.Left)
        {
            if (!IsSelecting || e.Shift) MoveLeft();
            if (!e.Shift) CollapseSelectionLeft();
        }
        else if (e.Key == Keys.Right)
        {
            if (!IsSelecting || e.Shift) MoveRight();
            if (!e.Shift) CollapseSelectionRight();
        }
        HandleTextEditShortcuts(e);
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
            return base.OnClickDown(mouse);
        }
        IsClicked = true;
        Vector2 convertedMouse = UIScene.ConvertMouseScreenCoords(mouse.Position);
        UIScene.FocusedComponent = this;
        caretLine = label.FindLineFromPos(mouse);
        caretIndex = convertedMouse.X <= label.Bounds.Z ? label.FindCaretIndexFromPos(mouse) : label.FindLineEndIndex(caretLine);
        desiredColumn = caretIndex;
        selectionAnchorIndex = caretIndex;
        selectionAnchorLine = caretLine;
        UpdateCaretPosition();
        SetCaretVisible();

        return true;
    }

    public override bool OnMouseMove(MouseState mouse)
    {
        if (WithinBounds(mouse)) UIScene.SetCursor(MouseCursor.IBeam);
        if (IsClicked)
        {
            Vector2 convertedMouse = UIScene.ConvertMouseScreenCoords(mouse.Position);
            caretLine = label.FindLineFromPos(mouse);
            caretIndex = convertedMouse.X <= label.Bounds.Z ? label.FindCaretIndexFromPos(mouse) : label.FindLineEndIndex(caretLine);
            desiredColumn = caretIndex;
            UpdateCaretPosition();
            ApplyAutoScroll();
            return true;
        }
        return base.OnMouseMove(mouse);
    }

    public override bool OnClickUp(MouseState mouse)
    {
        IsClicked = false;
        return base.OnClickUp(mouse);
    }

    public override void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
    {
        base.OnUpdate(deltaTime, mouse, keyboard);
        var convertedMouse = UIScene.ConvertMouseScreenCoords(mouse.Position);
        var view = ViewPort;
        if (IsClicked)
        {
            if (IsClicked && convertedMouse.X < view.X)
            {
                caretIndex = Math.Max(0, caretIndex - 1);
                UpdateCaretPosition();
                ApplyAutoScroll();
            }
            if (convertedMouse.Y > view.W)
            {
                caretLine = Math.Max(0, caretLine - 1);
                UpdateCaretPosition();
                ApplyAutoScroll();
            }
            if (convertedMouse.X > view.Z)
            {
                caretIndex = Math.Min(label.FindLineEndIndex(caretLine), caretIndex + 1);
                UpdateCaretPosition();
                ApplyAutoScroll();
            }
            if (convertedMouse.Y < view.Y)
            {
                caretLine = Math.Min(label.TotalLines - 1, caretLine + 1);
                UpdateCaretPosition();
                ApplyAutoScroll();
            }
        }
        UpdateBlinkTime(deltaTime);
    }

    public override void SubmitData(InstanceRenderer renderer)
    {
        if (!IsVisible) return;
        base.SubmitData(renderer);
        label.SubmitData(renderer);
        SubmitHighlightData(renderer);
        if (CaretVisible) renderer.AddInstance(Utils.Clip(caret, ViewPort));
    }
}