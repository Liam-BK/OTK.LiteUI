using OTK.LiteUI.UI.Rendering;

namespace OTK.LiteUI.UI.Interfaces
{
    /// <summary>
    /// Defines a component that can submit rendering data to a renderer.
    /// </summary>
    public interface IRenderable
    {
        /// <summary>
        /// Submits the component's render data to the specified renderer.
        /// </summary>
        /// <param name="renderer">The renderer receiving the data.</param>
        public void SubmitData(Renderer renderer);
    }
}