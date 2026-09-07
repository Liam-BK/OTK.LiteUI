using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.UI.Rendering;

public class Page : IRenderable, IUIContainer
{
    public List<UIComponent> Children { get; set; } = new List<UIComponent>();

    public void AddChild(UIComponent child)
    {
        throw new NotImplementedException();
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public void RemoveChild(UIComponent child)
    {
        throw new NotImplementedException();
    }

    public void SetLayout(ILayout layout)
    {
        throw new NotImplementedException();
    }

    public void SubmitData(Renderer renderer)
    {
        throw new NotImplementedException();
    }
}