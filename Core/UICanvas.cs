using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.UI.Rendering;
using OTK.LiteUI.UI.Rendering.Backend;
using OTK.LiteUI.UI.Utilities;

namespace OTK.LiteUI.Core
{
    public enum DisplayUnitType
    {
        /// <summary>
        /// Measurements are in absolute pixels.
        /// </summary>
        PX,
        /// <summary>
        /// Measurements are scaled according to the display DPI.
        /// </summary>
        DPI
    }

    public class UICanvas
    {
        public delegate Vector2 AnchorOffset(Vector2 canvasSize, UIComponent component);
        public AnchorOffset[] AnchorTypeOffsets = [
            //bottom left
            (_, _) => Vector2.Zero,
            //bottom center
            (canvas, component) => new Vector2(
                canvas.X * 0.5f, 0.0f
            ),
            //bottom right
            (canvas, component) => new Vector2(
                canvas.X, 0.0f
            ),
            //middle left
            (canvas, component) => new Vector2(
                0.0f, canvas.Y * 0.5f
            ),
            //center
            (canvas, component) => new Vector2(
                canvas.X, canvas.Y
            ) * 0.5f,
            //middle right
            (canvas, component) => new Vector2(
                canvas.X, canvas.Y * 0.5f
            ),
            //top left
            (canvas, component) => new Vector2(
                0.0f, canvas.Y
            ),
            //top center
            (canvas, component) => new Vector2(
                canvas.X * 0.5f, canvas.Y
            ),
            //top right
            (canvas, component) => new Vector2(
                canvas.X, canvas.Y
            ),
            //WholeWindow
            (canvas, component) => {
                component.Bounds = new Vector4(0, 0, canvas.X, canvas.Y);
                return Vector2.Zero;
            },
        ];
        private readonly GameWindow window;
        public static UICanvas? Default { get; set; } = null;
        private readonly TextureManager textureManager = new TextureManager();
        private readonly FontManager fontManager = new FontManager();
        private Renderer renderer;
        private Material material;
        public Matrix4 projection;
        public List<UIComponent> components = new List<UIComponent>();
        private List<UIComponent> pendingRemovals = new List<UIComponent>();
        internal Dictionary<string, FontData> Fonts => fontManager.Fonts;
        public static UIComponent? FocusedComponent = null;
        private bool cursorUpdated = false;
        private const float referenceDPI = 96.0f;

        public Vector2 CanvasSize => window.Size.ToVector2() * InvDPIScaleVec2;

        public string DefaultFontKey => fontManager.DefaultFontKey;

        public string ClipboardString
        {
            get
            {
                return window.ClipboardString;
            }
            set
            {
                window.ClipboardString = value;
            }
        }

        public Vector2 InvDPIScaleVec2
        {
            get => new Vector2(InvDPIScaleX, InvDPIScaleY);
        }

        /// <summary>
        /// The inverse DPI scale value for the X axis.
        /// </summary>
        public float InvDPIScaleX
        {
            get
            {
                return 1.0f / DPIScaleX;
            }
        }

        /// <summary>
        /// The inverse DPI scale value for the Y axis.
        /// </summary>
        public float InvDPIScaleY
        {
            get => 1.0f / DPIScaleY;
        }

        /// <summary>
        /// The DPI scale value for the X axis.
        /// </summary>
        public float DPIScaleX
        {
            get
            {
                return DisplayUnits == DisplayUnitType.DPI ? (window.CurrentMonitor.HorizontalDpi / referenceDPI) : 1;
            }
        }

        /// <summary>
        /// The DPI scale value for the X axis.
        /// </summary>
        public float DPIScaleY
        {
            get => DisplayUnits == DisplayUnitType.DPI ? (window.CurrentMonitor.VerticalDpi / referenceDPI) : 1;
        }

        public TextureResolution GetTextureResolution => textureManager.GetTextureResolution;

        private DisplayUnitType _displayUnits = DisplayUnitType.DPI;

        /// <summary>
        /// The current unit type used for measuring UI elements.
        /// Changing this will update the orthographic projection accordingly.
        /// </summary>
        public DisplayUnitType DisplayUnits
        {
            get
            {
                return _displayUnits;
            }
            set
            {
                _displayUnits = value;
                UpdateProjection();
            }
        }

        public UICanvas(GameWindow newWindow)
        {
            window = newWindow;
            var mesh = Mesh.Load("OTK.LiteUI.Assets.Meshes.Quad.obj");
            material = Material.Load("OTK.LiteUI.Assets.Materials.UIMaterial.mat", textureManager);
            InstanceAttribType[] attribTypes = [InstanceAttribType.Position2D, InstanceAttribType.Vec2, InstanceAttribType.TexCoords, InstanceAttribType.Vec2, InstanceAttribType.Color4, InstanceAttribType.Single];
            renderer = new Renderer(mesh, material, attribTypes);
            window.KeyDown += OnKeyDown;
            window.KeyUp += OnKeyUp;
            window.MouseDown += _ => OnClickDown(window.MouseState);
            window.MouseUp += _ => OnClickUp(window.MouseState);
            window.MouseMove += _ => OnMouseMove(window.MouseState);
            window.MouseWheel += _ => OnMouseWheel(window.MouseState);
            window.TextInput += OnTextInput;
            window.UpdateFrame += args => OnUpdate((float)args.Time, window.MouseState, window.KeyboardState);
            window.Resize += args => OnResize();
            window.Move += args => OnWindowMove();
            UpdateProjection();
            Default ??= this;
        }

        public bool ContainsTexture(string textureName)
        {
            return textureManager.ContainsTexture(textureName);
        }

        public void CreateTextureArray(int capacity, TextureResolution resolution)
        {
            textureManager.CreateTextureArray(capacity, resolution);
        }

        public void CreateTextureArray(TextureResolution resolution, int capacity)
        {
            textureManager.CreateTextureArray(capacity, resolution);
        }

        public bool TryLoadTexture(string path, string name, out TextureResolution finalResolution, TextureResolution? forceResolution = null, EmptyPixelType type = EmptyPixelType.Transparent, bool greyScale = false)
        {
            return textureManager.TryLoadTexture(path, name, out finalResolution, forceResolution, type, greyScale);
        }

        public bool LoadAllTextures(string directory, EmptyPixelType type, TextureResolution? forceResolution = null, bool greyScale = false)
        {
            return textureManager.LoadAllTextures(directory, type, forceResolution, greyScale);
        }

        public void UploadTexture(ImageData data, string name)
        {
            textureManager.UploadTexture(data, name);
        }

        public bool TryGetTexture(string name, out int layer)
        {
            return textureManager.TryGetTexture(name, out layer);
        }

        public void BindTextures(int textureUnit)
        {
            textureManager.Bind(textureUnit);
        }

        public bool TryDeleteTextures()
        {
            return textureManager.TryDeleteTextures();
        }

        public void LoadFont(string filePath, int fontSize = 32, TextureResolution resolution = TextureResolution.R512, char firstChar = ' ', char lastChar = '~')
        {
            fontManager.LoadFont(textureManager, filePath, fontSize, resolution, firstChar, lastChar);
        }

        private void UpdateProjection()
        {
            projection = Matrix4.CreateOrthographicOffCenter(0, window.Size.X * InvDPIScaleX, 0, window.Size.Y * InvDPIScaleY, 0.01f, 1.0f);
        }

        public void Register(UIComponent component)
        {
            components.Add(component);
        }

        public void Deregister(UIComponent component)
        {
            pendingRemovals.Add(component);
        }

        private void OnClickDown(MouseState mouse)
        {
            window.MakeCurrent();
            for (int i = components.Count - 1; i >= 0; i--)
            {
                var c = components[i];
                if (c.OnClickDown(mouse)) break;
            }
        }

        public void SetFocus(UIComponent? component)
        {
            if (FocusedComponent == component)
                return;

            FocusedComponent?.OnFocusLost();
            FocusedComponent = component;
            FocusedComponent?.OnFocusGained();
        }

        private void OnClickUp(MouseState mouse)
        {
            window.MakeCurrent();
            for (int i = components.Count - 1; i >= 0; i--)
            {
                var c = components[i];
                if (c.OnClickUp(mouse))
                {
                    if (c.CanFocus && c.IsVisible)
                    {
                        SetFocus(c);
                    }
                    else
                    {
                        SetFocus(null);
                    }
                    break;
                }
            }
        }

        private void OnKeyDown(KeyboardKeyEventArgs args)
        {
            window.MakeCurrent();
            for (int i = components.Count - 1; i >= 0; i--)
            {
                components[i].OnKeyDown(args);
            }
        }

        private void OnKeyUp(KeyboardKeyEventArgs args)
        {
            window.MakeCurrent();
            for (int i = components.Count - 1; i >= 0; i--)
            {
                components[i].OnKeyUp(args);
            }
        }

        private void OnTextInput(TextInputEventArgs args)
        {
            window.MakeCurrent();
            FocusedComponent?.OnTextInput(args);
        }

        private void OnMouseMove(MouseState mouse)
        {
            window.MakeCurrent();
            for (int i = components.Count - 1; i >= 0; i--)
            {
                var c = components[i];
                if (c.OnMouseMove(mouse)) break;
            }
            if (!cursorUpdated) window.Cursor = MouseCursor.Default;
            cursorUpdated = false;
        }

        private void OnMouseWheel(MouseState mouse)
        {
            window.MakeCurrent();
            for (int i = components.Count - 1; i >= 0; i--)
            {
                var c = components[i];
                if (c.OnMouseWheel(mouse)) break;
            }
        }

        private void OnUpdate(float delta, MouseState mouse, KeyboardState keys)
        {
            window.MakeCurrent();
            for (int i = components.Count - 1; i >= 0; i--)
            {
                components[i].OnUpdate(delta, mouse, keys);
            }
            material?.SetMatrix4("vpMatrix", projection);
            material?.UpdateUniforms();
            if (renderer is not null)
            {
                foreach (var component in components)
                {
                    if (component is IRenderable renderable) renderable.SubmitData(renderer);
                }
            }
            foreach (var component in pendingRemovals)
            {
                component.Deregister(components);
            }
            if (pendingRemovals.Count > 0) pendingRemovals.Clear();
        }

        public void OnResize()
        {
            window.MakeCurrent();
            UpdateProjection();
            GL.Viewport(0, 0, window.FramebufferSize.X, window.FramebufferSize.Y);
            DrawElements();
        }

        public void OnWindowMove()
        {

        }

        public void SetCursor(MouseCursor cursor)
        {
            window.MakeCurrent();
            cursorUpdated = true;
            window.Cursor = cursor;
        }

        public Vector2 ConvertMouseScreenCoords(Vector2 position)
        {
            return new Vector2(position.X, window.Size.Y - position.Y) * InvDPIScaleVec2;
        }

        public void DrawElements()
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.DepthTest);
            renderer?.DrawInstances(textureManager);
        }
    }
}