
using OTK.LiteUI.Core;

namespace OTK.LiteUI.UI.Components
{
    /// <summary>
    /// An empty UI component that can be used to provide extra spacing within a layout.
    /// </summary>
    public class Blank : UIComponent
    {
        /// <summary>
        /// Initializes a blank component on the specified canvas.
        /// </summary>
        /// <param name="canvas">The canvas to register the component with.</param>
        public Blank(UICanvas canvas)
        {
            Canvas = canvas;
            canvas.Register(this);
            if (GetType() == typeof(Blank)) Identifier = Canvas.GetNextIdentifier(typeof(Blank));
        }

        /// <summary>
        /// Initializes a blank component on the default canvas.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown when no default canvas is assigned.
        /// </exception>
        public Blank() : this(UICanvas.Default ?? throw new InvalidOperationException("Default Canvas must not be null for this Constructor. Try Creating a new UICanvas or assigning a UICanvas instance to UICanvas.Default")) { }
    }
}