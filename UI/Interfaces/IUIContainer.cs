using System.ComponentModel;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

public interface IUIContainer
{
    List<UIComponent> Children { get; }

    void AddChild(UIComponent child);
    void RemoveChild(UIComponent child);

    void SetLayout(ILayout layout);
}