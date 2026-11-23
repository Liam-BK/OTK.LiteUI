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

    /// <summary>
    /// The root of all user interfaces in FastUI. Manages UI elements, rendering, input handling, and resources for a window.
    /// </summary>
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
        ];
        public readonly GameWindow window;

        /// <summary>
        /// Contains a reference to the Default UICanvas. It is automatically set the first time a UICanvas is created, but can be manually changed later.
        /// </summary>
        public static UICanvas? Default { get; set; } = null;
        private readonly TextureManager textureManager = new TextureManager();
        private readonly FontManager fontManager = new FontManager();
        private Renderer renderer;
        private Material material;
        public Matrix4 projection;
        public List<UIElement> elements = new List<UIElement>();
        private List<UIElement> pendingRemovals = new List<UIElement>();
        internal Dictionary<string, FontData> Fonts => fontManager.Fonts;
        internal Dictionary<Type, int> ComponentCounts = new Dictionary<Type, int>();
        public static UIComponent? FocusedComponent = null;
        internal bool cursorUpdated = false;
        private const float referenceDPI = 96.0f;
        private bool firstFrame = true;

        /// <summary>
        /// The total number of occupied layers in the TextureArray.
        /// </summary>
        public int TextureCount => textureManager.TextureCount;

        /// <summary>
        /// The total number of occupied and free layers in the TextureArray.
        /// </summary>
        public int TextureCapacity => textureManager.TextureCapacity;

        /// <summary>
        /// Only returns all elements directly stored on the Canvas. Does not count nested elements.
        /// </summary>
        public int RegisteredElements => elements.Count;

        /// <summary>
        /// Returns the canvas' current dimensions in the current display units.
        /// </summary>
        public Vector2 CanvasSize => window.Size.ToVector2() * InvDPIScaleVec2;

        /// <summary>
        /// References the default dictionary key for a font. It is automatically assigned the first time a font is loaded.
        /// </summary>
        public string DefaultFontKey => fontManager.DefaultFontKey;

        /// <summary>
        /// Accesses the window clipboard, allowing copy and paste between FastUI and the OS.
        /// </summary>
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

        /// <summary>
        /// Prevents the Cursor appearance from changing back to the default mouse cursor. Useful for click and drag effects.
        /// </summary>
        public bool LockCursorAppearance { get; set; }

        /// <summary>
        /// Returns the inverse DPI scale for both axes. Useful for screen coordinate calculations.
        /// </summary>
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

        private float _windowDPIX;

        /// <summary>
        /// The DPI scale value for the X axis.
        /// </summary>
        public float DPIScaleX
        {
            get
            {
                return DisplayUnits == DisplayUnitType.DPI ? (_windowDPIX / referenceDPI) : 1;
            }
        }

        private float _windowDPIY;

        /// <summary>
        /// The DPI scale value for the Y axis.
        /// </summary>
        public float DPIScaleY
        {
            get => DisplayUnits == DisplayUnitType.DPI ? (_windowDPIY / referenceDPI) : 1;
        }

        /// <summary>
        /// Returns the resolution of the underlying texture array.
        /// </summary>
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

        /// <summary>
        /// Creates a UI canvas associated with the specified game window.
        /// </summary>
        /// <param name="newWindow">The window the canvas will accept input from and render to.</param>
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
            window.MouseWheel += _ => OnMouseWheel(window.MouseState);
            window.MouseMove += _ => OnMouseMove(window.MouseState);
            window.MouseUp += _ => OnClickUp(window.MouseState);
            window.TextInput += OnTextInput;
            window.UpdateFrame += args => OnUpdate((float)args.Time, window.MouseState, window.KeyboardState);
            window.Resize += args => OnResize();
            window.Move += args => OnWindowMove();
            UpdateProjection();
            _windowDPIY = window.CurrentMonitor.VerticalDpi;
            _windowDPIX = window.CurrentMonitor.HorizontalDpi;
            Default ??= this;
            AutoRegistry.Initialize();
        }

        /// <summary>
        /// Similar to RegisteredElements, but also filters for the specified type T.
        /// </summary>
        /// <typeparam name="T">The type of UIElement you want to find the count for.</typeparam>
        /// <returns></returns>
        public int DirectElementCount<T>() where T : UIElement
        {
            int result = 0;
            foreach (var element in elements)
            {
                if (element.GetType() == typeof(T)) result++;
            }
            return result;
        }

        /// <summary>
        /// Checks whether a texture with the specified name is loaded.
        /// </summary>
        /// <param name="textureName">The name of the texture to find.</param>
        /// <returns><see langword="true"/> if the texture exists. Otherwise, <see langword="false"/>.</returns>
        public bool ContainsTexture(string textureName)
        {
            return textureManager.ContainsTexture(textureName);
        }

        /// <summary>
        /// Creates a texture array with the specified capacity and resolution. A texture array must be created before loading a font or texture.
        /// </summary>
        /// <param name="capacity">The maximum number of texture layers.</param>
        /// <param name="resolution">The  resolution of the texture array.</param>
        public void CreateTextureArray(int capacity, TextureResolution resolution)
        {
            textureManager.CreateTextureArray(capacity, resolution);
        }

        /// <summary>
        /// Creates a texture array with the specified capacity and resolution. A texture array must be created before loading a font or texture.
        /// </summary>
        /// <param name="capacity">The maximum number of texture layers.</param>
        /// <param name="resolution">The  resolution of the texture array.</param>
        public void CreateTextureArray(TextureResolution resolution, int capacity)
        {
            textureManager.CreateTextureArray(capacity, resolution);
        }

        /// <summary>
        /// Attempts to load a texture from a file into the texture array.
        /// </summary>
        /// <param name="path">The path to the texture file.</param>
        /// <param name="name">The name used to identify the loaded texture.</param>
        /// <param name="forceResolution">Optional resolution to use instead of the texture's original resolution.</param>
        /// <param name="type">The pixel type used to fill space if the supplied texture is of a smaller resolution than the texture layer.</param>
        /// <param name="greyScale">Whether the texture should be converted to greyscale.</param>
        /// <returns><see langword="true"/> if the texture was loaded successfully. Otherwise, returns <see langword="false"/>.</returns>
        public bool TryLoadTexture(string path, string name, TextureResolution? forceResolution = null, EmptyPixelType type = EmptyPixelType.Transparent, bool greyScale = false)
        {
            return textureManager.TryLoadTexture(path, name, forceResolution, type, greyScale);
        }

        /// <summary>
        /// Attempts to load all supported textures from the specified directory.
        /// </summary>
        /// <param name="directory">The directory containing the texture files.</param>
        /// <param name="forceResolution">Optional resolution to use for all loaded textures.</param>
        /// <param name="type">The pixel type used to fill space if the supplied texture is of a smaller resolution than the texture layer.</param>
        /// <param name="greyScale">Whether the textures should be converted to greyscale.</param>
        /// <returns><see langword="true"/> if the textures are loaded successfully. Otherwise, returns <see langword="false"/>.</returns>
        public bool LoadAllTextures(string directory, TextureResolution? forceResolution = null, EmptyPixelType type = EmptyPixelType.Transparent, bool greyScale = false)
        {
            return textureManager.LoadAllTextures(directory, type, forceResolution, greyScale);
        }

        /// <summary>
        /// Uploads image data to the texture array under the specified name.
        /// </summary>
        /// <param name="data">The image data to upload.</param>
        /// <param name="name">The name used to identify the texture.</param>
        public void UploadTexture(ImageData data, string name)
        {
            textureManager.UploadTexture(data, name);
        }

        /// <summary>
        /// Attempts to retrieve the texture array layer assigned to the specified texture name.
        /// </summary>
        /// <param name="name">The name of the texture.</param>
        /// <param name="layer">Receives the texture's layer index if found.</param>
        /// <returns><see langword="true"/> if the texture exists. Otherwise, returns <see langword="false"/>.</returns>
        public bool TryGetTexture(string name, out int layer)
        {
            return textureManager.TryGetTexture(name, out layer);
        }

        /// <summary>
        /// Binds the texture array to the specified texture unit.
        /// </summary>
        /// <param name="textureUnit">The OpenGL texture unit to bind to.</param>
        public void BindTextures(int textureUnit)
        {
            textureManager.Bind(textureUnit);
        }

        /// <summary>
        /// Attempts to delete the texture array and its associated resources.
        /// </summary>
        /// <returns><see langword="true"/> if the resources were successfully deleted. Otherwise, returns <see langword="false"/>.</returns>
        public bool TryDeleteTextures()
        {
            return textureManager.TryDeleteTextures();
        }

        /// <summary>
        /// Loads a font and generates its character textures, as well as additional font data. A texture array must already exist for this operation to succeed.
        /// </summary>
        /// <param name="filePath">The path to the font file.</param>
        /// <param name="fontSize">The pixel size of the characters on the atlas.</param>
        /// <param name="resolution">The resolution of the generated character texture.</param>
        /// <param name="firstChar">The first character to include.</param>
        /// <param name="lastChar">The last character to include.</param>
        public void LoadFont(string filePath, int fontSize = 32, TextureResolution resolution = TextureResolution.R512, char firstChar = ' ', char lastChar = '~')
        {
            fontManager.LoadFont(textureManager, filePath, fontSize, resolution, firstChar, lastChar);
        }

        private void UpdateProjection()
        {
            projection = Matrix4.CreateOrthographicOffCenter(0, window.Size.X * InvDPIScaleX, 0, window.Size.Y * InvDPIScaleY, 0.01f, 1.0f);
        }

        /// <summary>
        /// Registers an element with the canvas. Custom UIComponents should have this inside their constructor.
        /// </summary>
        /// <param name="element">The element to register.</param>
        public void Register(UIElement element)
        {
            elements.Add(element);
        }

        /// <summary>
        /// Schedules an element for deregistration from the canvas. It should typically be called when element becomes managed by another component.
        /// </summary>
        /// <param name="element">The element to deregister.</param>
        public void Deregister(UIElement element)
        {
            pendingRemovals.Add(element);
        }

        /// <summary>
        /// Generates the next automatically assigned identifier for the specified element type. Not mandatory for custom components, but recommended.
        /// </summary>
        /// <param name="type">The type of element to generate an identifier for.</param>
        /// <returns>A unique identifier based on the element type and its creation count.</returns>
        public string GetNextIdentifier(Type type)
        {
            if (ComponentCounts.ContainsKey(type)) ComponentCounts[type] = ComponentCounts[type] + 1;
            else ComponentCounts.Add(type, 0);
            var result = $"{type.Name}_{ComponentCounts[type]}";
            Console.WriteLine($"{type.Name}_{ComponentCounts[type]}");
            return result;
        }

        /// <summary>
        /// Attempts to find an element by its identifier path.
        /// </summary>
        /// <typeparam name="T">The expected element type.</typeparam>
        /// <param name="identifierPath">The identifier path, using the specified delimiter to separate hierarchy levels.</param>
        /// <param name="delimiter">The delimiter used to separate identifiers. Defaults to a period.</param>
        /// <returns>The matching element, or <see langword="null"/> if no matching element is found.</returns>
        public T? Get<T>(string identifierPath, string delimiter = ".") where T : UIElement
        {
            var identifiers = identifierPath.Split(delimiter);
            foreach (var component in elements)
            {
                if (component.Identifier != identifiers[0]) continue;
                if (identifiers.Length == 1) return component as T;
                if (component is IUIContainer container) return Get<T>(identifiers, 1, container);
                return null;
            }
            return null;
        }

        private T? Get<T>(string[] identifiers, int index, IUIContainer container) where T : UIElement
        {
            foreach (var component in container.Children)
            {
                if (component.Identifier != identifiers[index]) continue;
                if (index == identifiers.Length - 1) return component as T;
                if (component is IUIContainer next) return Get<T>(identifiers, index + 1, next);
                return null;
            }
            return null;
        }

        /// <summary>
        /// Finds an element by its identifier path.
        /// </summary>
        /// <typeparam name="T">The expected element type.</typeparam>
        /// <param name="identifierPath">The identifier path, using the specified delimiter to separate hierarchy levels.</param>
        /// <param name="delimiter">The delimiter used to separate identifiers. Defaults to a period.</param>
        /// <returns>The matching element.</returns>
        /// <exception cref="UIComponentNotFoundException">
        /// Thrown when the identifier path cannot be resolved or the element is not of the expected type.
        /// </exception>
        public T GetStrict<T>(string identifierPath, string delimiter = ".") where T : UIElement
        {
            var identifiers = identifierPath.Split(delimiter);
            var failPoint = identifiers[0];
            foreach (var component in elements)
            {
                if (component.Identifier != identifiers[0]) continue;
                if (identifiers.Length == 1)
                {
                    var result = component as T;
                    var resultType = component.GetType();
                    if (result == null) throw new UIComponentNotFoundException(failPoint, resultType, typeof(T));
                    return result;
                }
                if (component is IUIContainer container) return GetStrict<T>(identifiers, 1, failPoint, delimiter, container);
                throw new UIComponentNotFoundException(failPoint);
            }
            throw new UIComponentNotFoundException(failPoint);
        }

        private T GetStrict<T>(string[] identifiers, int index, string resolvedPath, string delimiter, IUIContainer container) where T : UIElement
        {
            var failPoint = identifiers[index];
            foreach (var component in container.Children)
            {
                if (component.Identifier != identifiers[index]) continue;
                if (index == identifiers.Length - 1)
                {
                    var result = component as T;
                    var resultType = component.GetType();
                    if (result == null) throw new UIComponentNotFoundException(failPoint, resultType, typeof(T));
                    return result;
                }
                if (component is IUIContainer next) return GetStrict<T>(identifiers, index + 1, $"{resolvedPath}{delimiter}{failPoint}", delimiter, next);
                throw new UIComponentNotFoundException($"{resolvedPath}{delimiter}{failPoint}");
            }
            throw new UIComponentNotFoundException($"{resolvedPath}{delimiter}{failPoint}");
        }

        /// <summary>
        /// Loads UI elements from a JSON stream and adds them to the canvas.
        /// </summary>
        /// <param name="inputStream">The stream containing the UI definition.</param>
        public void LoadJsonUI(Stream inputStream)
        {
            var loader = new UIJsonLoader(this);
            loader.Load(inputStream);
        }

        /// <summary>
        /// Sets the currently focused UI component.
        /// </summary>
        /// <param name="component">The component to focus, or <see langword="null"/> to clear focus.</param>
        public void SetFocus(UIComponent? component)
        {
            if (FocusedComponent == component)
                return;

            FocusedComponent?.OnFocusLost();
            FocusedComponent = component;
            FocusedComponent?.OnFocusGained();
        }

        private void OnClickDown(MouseState mouse)
        {
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                var component = elements[i];
                if (component.OnClickDown(mouse))
                {
                    elements[i] = elements[^1];
                    elements[^1] = component;
                    break;
                }
            }

            foreach (var element in elements)
            {
                element.OnClickDownNoConsume(mouse);
            }
        }

        private void OnMouseWheel(MouseState mouse)
        {
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                var c = elements[i];
                if (c.OnMouseWheel(mouse)) break;
            }

            foreach (var element in elements)
            {
                element.OnMouseWheelNoConsume(mouse);
            }
        }

        private void OnMouseMove(MouseState mouse)
        {
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                var c = elements[i];
                if (c.OnMouseMove(mouse)) break;
            }
            if (!LockCursorAppearance)
            {
                if (!cursorUpdated) window.Cursor = MouseCursor.Default;
                cursorUpdated = false;
            }

            foreach (var element in elements)
            {
                element.OnMouseMoveNoConsume(mouse);
            }
        }

        private void OnClickUp(MouseState mouse)
        {
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                var c = elements[i];
                if (c.OnClickUp(mouse))
                {
                    break;
                }
            }

            foreach (var element in elements)
            {
                element.OnClickUpNoConsume(mouse);
            }
        }

        private void OnKeyDown(KeyboardKeyEventArgs args)
        {
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                elements[i].OnKeyDown(args);
            }
        }

        private void OnKeyUp(KeyboardKeyEventArgs args)
        {
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                elements[i].OnKeyUp(args);
            }
        }

        private void OnTextInput(TextInputEventArgs args)
        {
            FocusedComponent?.OnTextInput(args);
        }

        private void OnUpdate(float delta, MouseState mouse, KeyboardState keys)
        {
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                elements[i].OnUpdate(delta, mouse, keys);
            }
            material?.SetMatrix4("vpMatrix", projection);
            material?.UpdateUniforms();
            if (renderer is not null)
            {
                foreach (var component in elements)
                {
                    if (component is IRenderable renderable) renderable.SubmitData(renderer);
                }
            }
            for (int i = pendingRemovals.Count - 1; i >= 0; i--)
            {
                elements.Remove(pendingRemovals[i]);
            }

            pendingRemovals.Clear();
        }

        private void OnResize()
        {
            window.MakeCurrent();
            UpdateProjection();
            GL.Viewport(0, 0, window.FramebufferSize.X, window.FramebufferSize.Y);
        }

        private void OnWindowMove()
        {

        }

        /// <summary>
        /// Sets the mouse cursor appearance for the assigned window.
        /// </summary>
        /// <param name="cursor">The cursor to display.</param>
        public void SetCursor(MouseCursor cursor)
        {
            cursorUpdated = true;
            window.Cursor = cursor;
        }

        /// <summary>
        /// Converts screen-space mouse coordinates into canvas-space coordinates.
        /// </summary>
        /// <param name="position">The mouse position in screen coordinates.</param>
        /// <returns>The converted position in canvas coordinates.</returns>
        public Vector2 ConvertMouseScreenCoords(Vector2 position)
        {
            return new Vector2(position.X, window.ClientSize.Y - position.Y) * InvDPIScaleVec2;
        }

        /// <summary>
        /// Draws all registered UI elements.
        /// </summary>
        public void DrawElements()
        {
            if (firstFrame)
            {
                firstFrame = false;
                return;
            }

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
            GL.Disable(EnableCap.DepthTest);
            renderer?.DrawInstances(textureManager);
        }
    }

    /// <summary>
    /// Represents an error that occurs when a UI component cannot be found during a Get<t>() operation or does not match the expected type.
    /// </summary>
    public class UIComponentNotFoundException : Exception
    {
        /// <summary>
        /// Initializes an exception for a component that was found with an unexpected type.
        /// </summary>
        /// <param name="path">The identifier path of the component.</param>
        /// <param name="found">The actual component type.</param>
        /// <param name="expected">The expected component type.</param>
        public UIComponentNotFoundException(string path, Type found, Type expected) : base($"UI component '{path}' was found with wrong Type. Found Type: {found}, Expected Type: {expected}") { }

        /// <summary>
        /// Initializes an exception for a component that could not be found.
        /// </summary>
        /// <param name="pathFailPoint">The identifier path or segment where the lookup failed.</param>
        public UIComponentNotFoundException(string pathFailPoint) : base($"Could not find UI Component matching path at: {pathFailPoint}") { }
    }
}