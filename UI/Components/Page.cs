using System.ComponentModel;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.UI.Rendering;

public class Page : UIElement, IRenderable, IUIContainer
{
    public List<UIComponent> Children { get; set; } = new List<UIComponent>();

    private bool _isVisible = true;

    [UIProperty]
    public override bool IsVisible
    {
        get => _isVisible;
        set
        {
            _isVisible = value;
        }
    }

    public Page(UICanvas canvas)
    {
        Canvas = canvas;
        Canvas.Register(this);
        if (GetType() == typeof(Page)) Identifier = Canvas.GetNextIdentifier(typeof(Page));
    }

    public Page() : this(UICanvas.Default ?? throw new InvalidOperationException("Default Canvas must not be null for this Constructor. Try Creating a new UICanvas or assigning a UICanvas instance to UICanvas.Default")) { }

    public void AddChild(UIComponent child)
    {
        Canvas.Deregister(child);
        child.Parent = this;
        child.IsVisible = IsVisible;
        Children.Add(child);
    }

    public void Clear()
    {
        Children.Clear();
    }

    public void RemoveChild(UIComponent child)
    {
        Children.Remove(child);
    }

    public void SetLayout(ILayout layout) { }

    public void SubmitData(Renderer renderer)
    {
        if (!IsVisible) return;
        foreach (var child in Children)
        {
            if (child is IRenderable renderable) renderable.SubmitData(renderer);
        }
    }

    public override bool OnClickDown(MouseState mouse)
    {
        if (!IsVisible) return false;
        for (int i = Children.Count - 1; i >= 0; i--)
        {
            var element = Children[i];
            if (element.OnClickDown(mouse))
            {
                Children[i] = Children[^1];
                Children[^1] = element;
                return true;
            }
        }
        return base.OnClickDown(mouse);
    }

    public override void OnClickDownNoConsume(MouseState mouse)
    {
        if (!IsVisible) return;
        foreach (var element in Children)
        {
            element.OnClickDownNoConsume(mouse);
        }
        base.OnClickDownNoConsume(mouse);
    }

    public override bool OnClickUp(MouseState mouse)
    {
        if (!IsVisible) return false;
        foreach (var element in Children)
        {
            bool result = element.OnClickUp(mouse);
            if (result) return true;
        }
        return base.OnClickUp(mouse);
    }

    public override void OnClickUpNoConsume(MouseState mouse)
    {
        if (!IsVisible) return;
        foreach (var element in Children)
        {
            element.OnClickUpNoConsume(mouse);
        }
        base.OnClickUpNoConsume(mouse);
    }

    public override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        if (!IsVisible) return;
        foreach (var element in Children)
        {
            element.OnKeyDown(e);
        }
    }

    public override void OnKeyUp(KeyboardKeyEventArgs e)
    {
        if (!IsVisible) return;
        foreach (var element in Children)
        {
            element.OnKeyUp(e);
        }
    }

    public override void OnTextInput(TextInputEventArgs e)
    {
        if (!IsVisible) return;
        foreach (var element in Children)
        {
            element.OnTextInput(e);
        }
    }

    public override bool OnMouseMove(MouseState mouse)
    {
        if (!IsVisible) return false;
        foreach (var element in Children)
        {
            bool result = element.OnMouseMove(mouse);
            if (result) return true;
        }
        return base.OnMouseMove(mouse);
    }

    public override void OnMouseMoveNoConsume(MouseState mouse)
    {
        if (!IsVisible) return;
        foreach (var element in Children)
        {
            element.OnMouseMoveNoConsume(mouse);
        }
        base.OnMouseMoveNoConsume(mouse);
    }

    public override bool OnMouseWheel(MouseState mouse)
    {
        if (!IsVisible) return false;
        foreach (var element in Children)
        {
            bool result = element.OnMouseWheel(mouse);
            if (result)
            {
                return true;
            }
        }
        return base.OnMouseWheel(mouse);
    }

    public override void OnMouseWheelNoConsume(MouseState mouse)
    {
        if (!IsVisible) return;
        foreach (var element in Children)
        {
            element.OnMouseWheelNoConsume(mouse);
        }
        base.OnMouseWheelNoConsume(mouse);
    }

    public override void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
    {
        if (!IsVisible) return;
        foreach (var element in Children)
        {
            element.OnUpdate(deltaTime, mouse, keyboard);
        }
    }
}