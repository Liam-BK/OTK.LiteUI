using OTK.LiteUI.UI.Components;

namespace OTK.LiteUI.UI.Interfaces
{
    public interface IUIContainer
    {
        List<UIComponent> Children { get; }

        void AddChild(UIComponent child);

        void RemoveChild(UIComponent child);

        void Clear();

        void SetLayout(ILayout layout);
    }
}