using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public class Panel : NineSlice, IUIContainer
{
    private List<UIComponent> _children = new List<UIComponent>();
    public List<UIComponent> Children => _children;

    public ILayout Layout
    {
        private get;
        set;
    }

    public Panel(Vector4 bounds, ILayout layout, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(bounds, inset, uvInset, colour)
    {
        Layout = layout;
    }


    public void AddChild(UIComponent child)
    {
        UIScene.Deregister(child);
        child.Parent = this;
        child.ClipBounds = Layout.LayoutBounds;
        Children.Add(child);
        Layout.Apply(Children);
    }

    public void SetLayout(ILayout layout)
    {
        Layout = layout;
    }

    public void RemoveChild(UIComponent child)
    {
        Children.Remove(child);
        Layout.Apply(Children);
    }

    public override bool OnClickDown(MouseState mouse)
    {
        if (!IsVisible || !WithinBounds(mouse)) return false;
        foreach (var element in Children)
        {
            bool result = element.OnClickDown(mouse);
            if (result) return true;
        }
        return base.OnClickDown(mouse);
    }

    public override bool OnClickUp(MouseState mouse)
    {
        if (!IsVisible || !WithinBounds(mouse)) return false;
        foreach (var element in Children)
        {
            bool result = element.OnClickUp(mouse);
            if (result) return true;
        }
        return base.OnClickUp(mouse);
    }

    public override bool OnMouseMove(MouseState mouse)
    {
        if (!IsVisible || !WithinBounds(mouse)) return false;
        foreach (var element in Children)
        {
            bool result = element.OnMouseMove(mouse);
            if (result) return true;
        }
        return base.OnMouseMove(mouse);
    }

    public override bool OnMouseWheel(MouseState mouse)
    {
        if (!IsVisible) return false;
        foreach (var element in Children)
        {
            bool result = element.OnMouseWheel(mouse);
            if (result) return true;
        }
        Layout.Apply(Children);
        return base.OnMouseWheel(mouse);
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

    public override void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
    {
        if (!IsVisible) return;
        if (Layout is not null) Layout.Apply(Children);
        foreach (var element in Children)
        {
            element.OnUpdate(deltaTime, mouse, keyboard);
        }
    }

    public override void SubmitData(InstanceRenderer renderer)
    {
        if (!IsVisible) return;
        base.SubmitData(renderer);
        foreach (var element in Children)
        {
            if (element is IRenderable renderable)
            {
                renderable.SubmitData(renderer);
            }
        }
    }
}