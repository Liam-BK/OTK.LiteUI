using OTK.LiteUI.UI.Components;

namespace OTK.LiteUI.UI.Interfaces
{
    /// <summary>
    /// Defines a component that can contain and manage child UI components.
    /// </summary>
    public interface IUIContainer
    {
        /// <summary>
        /// The child components managed by this container.
        /// </summary>
        List<UIComponent> Children { get; }

        /// <summary>
        /// Adds a component to the container.
        /// </summary>
        /// <param name="child">The component to add.</param>
        void AddChild(UIComponent child);

        /// <summary>
        /// Removes a component from the container.
        /// </summary>
        /// <param name="child">The component to remove.</param>
        void RemoveChild(UIComponent child);

        /// <summary>
        /// Removes all child components from the container.
        /// </summary>
        void Clear();

        /// <summary>
        /// Sets the layout used to arrange the container's child components.
        /// </summary>
        /// <param name="layout">The layout to apply to the children.</param>
        void SetLayout(ILayout layout);
    }
}