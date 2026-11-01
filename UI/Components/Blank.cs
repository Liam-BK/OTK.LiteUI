
using OTK.LiteUI.Core;

namespace OTK.LiteUI.UI.Components
{
    public class Blank : UIComponent
    {
        public Blank(UICanvas canvas)
        {
            Canvas = canvas;
            canvas.Register(this);
            if (GetType() == typeof(Blank)) Identifier = Canvas.GetNextIdentifier(typeof(Blank));
        }

        public Blank() : this(UICanvas.Default ?? throw new InvalidOperationException("Default Canvas must not be null for this Constructor. Try Creating a new UICanvas or assigning a UICanvas instance to UICanvas.Default")) { }
    }
}