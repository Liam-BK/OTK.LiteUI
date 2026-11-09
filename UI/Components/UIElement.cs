using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OTK.LiteUI.Core;

namespace OTK.LiteUI.UI.Components
{
    public abstract class UIElement
    {
        public UICanvas Canvas = null!;

        [UIProperty]
        public virtual bool IsVisible { get; set; } = true;

        [UIProperty]
        public string Identifier
        {
            get;
            set;
        } = "";

        public virtual bool OnClickDown(MouseState mouse)
        {
            return false;
        }

        public virtual void OnClickDownNoConsume(MouseState mouse)
        {

        }

        public virtual bool OnClickUp(MouseState mouse)
        {
            return false;
        }

        public virtual void OnClickUpNoConsume(MouseState mouse)
        {

        }

        public virtual bool OnMouseMove(MouseState mouse)
        {
            return false;
        }

        public virtual void OnMouseMoveNoConsume(MouseState mouse)
        {

        }

        public virtual bool OnMouseWheel(MouseState mouse)
        {
            return false;
        }

        public virtual void OnMouseWheelNoConsume(MouseState mouse)
        {

        }

        public virtual void OnTextInput(TextInputEventArgs e)
        {

        }

        public virtual void OnKeyDown(KeyboardKeyEventArgs e)
        {

        }

        public virtual void OnKeyUp(KeyboardKeyEventArgs e)
        {

        }

        public virtual void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
        {

        }

        public virtual void Deregister(List<UIElement> registry)
        {
            if (!registry.Remove(this)) throw new InvalidOperationException("Attempted to deregister a UI element that was not registered.");
        }
    }
}