using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.UI.Rendering;

public class Page : IRenderable, IUIContainer
{
    public List<UIComponent> Children { get; set; } = new List<UIComponent>();
    public bool IsVisible { get; set; } = false;

    public void AddChild(UIComponent child)
    {
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