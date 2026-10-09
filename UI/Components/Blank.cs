
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
    }
}