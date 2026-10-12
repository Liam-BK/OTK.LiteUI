using System.ComponentModel;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.UI.Rendering;

public class Page : UIElement, IRenderable, IUIContainer
{
    public List<UIComponent> Children { get; set; } = new List<UIComponent>();
    private bool _isVisible = true;
    public override bool IsVisible
    {
        get => _isVisible;
        set
        {
            _isVisible = value;
            foreach (var child in Children)
            {
                child.IsVisible = value;
            }
        }
    }

    public Page(UICanvas canvas)
    {
        Canvas = canvas;
        Canvas.Register(this);
        if (GetType() == typeof(Page)) Identifier = Canvas.GetNextIdentifier(typeof(Page));
    }

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
}