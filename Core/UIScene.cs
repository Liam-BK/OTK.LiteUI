using System.Reflection;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

public static class UIScene
{
    public static GameWindow? window = null;
    private static bool _initialized = false;
    public static bool Initialized => _initialized;
    private static InstanceRenderer? renderer;
    private static Material? material;
    public static Matrix4 projection;
    public static TextureResolution resolution = TextureResolution.R256;
    private const float referenceDPI = 96.0f;
    public static List<UIComponent> components = new List<UIComponent>();
    public static List<UIComponent> pendingRemovals = new List<UIComponent>();

    /// <summary>
    /// A Vector2 that can be multiplied with a position to multiply it by the inverse DPI scale.
    /// </summary>
    public static Vector2 InvDPIScaleVec2
    {
        get => new Vector2(InvDPIScaleX, InvDPIScaleY);
    }

    /// <summary>
    /// The inverse DPI scale value for the X axis.
    /// </summary>
    public static float InvDPIScaleX
    {
        get
        {
            return 1.0f / DPIScaleX;
        }
    }

    /// <summary>
    /// The inverse DPI scale value for the Y axis.
    /// </summary>
    public static float InvDPIScaleY
    {
        get => 1.0f / DPIScaleY;
    }

    /// <summary>
    /// The DPI scale value for the X axis.
    /// </summary>
    public static float DPIScaleX
    {
        get
        {
            return window is not null && DisplayUnits == DisplayUnitType.DPI ? (window.CurrentMonitor.HorizontalDpi / referenceDPI) : 1;
        }
    }

    /// <summary>
    /// The DPI scale value for the X axis.
    /// </summary>
    public static float DPIScaleY
    {
        get => window is not null && DisplayUnits == DisplayUnitType.DPI ? (window.CurrentMonitor.VerticalDpi / referenceDPI) : 1;
    }

    private static DisplayUnitType _displayUnits = DisplayUnitType.DPI;

    /// <summary>
    /// The current unit type used for measuring UI elements.
    /// Changing this will update the orthographic projection accordingly.
    /// </summary>
    public static DisplayUnitType DisplayUnits
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
    /// Represents the unit type used for UI element measurements.
    /// </summary>
    public enum DisplayUnitType
    {
        /// <summary>
        /// Measurements are in absolute pixels.
        /// </summary>
        Pixels,
        /// <summary>
        /// Measurements are scaled according to the display DPI.
        /// </summary>
        DPI
    }

    public static UIComponent? FocusedComponent = null;

    public static void Initialize(GameWindow newWindow, TextureResolution resolution)
    {
        if (_initialized) return;
        _initialized = true;
        window = newWindow;
        var mesh = Mesh.Load("/Users/liam/VS Code Projects/OTK.LiteUI/Assets/Meshes/Quad.obj");
        material = Material.Load("/Users/liam/VS Code Projects/OTK.LiteUI/Assets/Materials/UIMaterial.mat");
        InstanceAttribType[] attribTypes = [InstanceAttribType.Position2D, InstanceAttribType.Vec2, InstanceAttribType.TexCoords, InstanceAttribType.Vec2, InstanceAttribType.Color4, InstanceAttribType.Single];
        renderer = new InstanceRenderer(mesh, material, attribTypes);
        window.KeyDown += OnKeyDown;
        window.KeyUp += OnKeyUp;
        window.MouseDown += _ => OnClickDown(window.MouseState);
        window.MouseUp += _ => OnClickUp(window.MouseState);
        window.MouseMove += _ => OnMouseMove(window.MouseState);
        window.MouseWheel += _ => OnMouseWheel(window.MouseState);
        window.TextInput += OnTextInput;
        window.UpdateFrame += args => OnUpdate((float)args.Time, window.MouseState, window.KeyboardState);
        UpdateProjection();
    }

    private static void UpdateProjection()
    {
        if (!_initialized || window is null) throw new InvalidOperationException("UIScene has not been initialized. Make sure to call 'Initialize' before doing any operations.");
        projection = Matrix4.CreateOrthographicOffCenter(0, window.Size.X * InvDPIScaleX, 0, window.Size.Y * InvDPIScaleY, 0.01f, 1.0f);
    }

    public static void Register(UIComponent component)
    {
        if (!_initialized) throw new InvalidOperationException("UIScene has not been initialized. Make sure to call 'Initialize' before doing any operations.");
        components.Add(component);
    }

    public static void Deregister(UIComponent component)
    {
        if (!_initialized) throw new InvalidOperationException("UIScene has not been initialized. Make sure to call 'Initialize' before doing any operations.");
        pendingRemovals.Add(component);
    }

    public static void OnClickDown(MouseState mouse)
    {
        if (!_initialized) return;
        for (int i = components.Count - 1; i >= 0; i--)
        {
            var c = components[i];
            if (c.OnClickDown(mouse)) break;
        }
    }

    public static void SetFocus(UIComponent? component)
    {
        if (FocusedComponent == component)
            return;

        FocusedComponent?.OnFocusLost();
        FocusedComponent = component;
        FocusedComponent?.OnFocusGained();
    }

    public static void OnClickUp(MouseState mouse)
    {
        if (!_initialized) return;
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

    public static void OnKeyDown(KeyboardKeyEventArgs args)
    {
        if (!_initialized) return;
        for (int i = components.Count - 1; i >= 0; i--)
        {
            components[i].OnKeyDown(args);
        }
    }

    public static void OnKeyUp(KeyboardKeyEventArgs args)
    {
        if (!_initialized) return;
        for (int i = components.Count - 1; i >= 0; i--)
        {
            components[i].OnKeyUp(args);
        }
    }

    public static void OnTextInput(TextInputEventArgs args)
    {
        if (!_initialized) return;
        if (FocusedComponent is not null)
        {
            FocusedComponent.OnTextInput(args);
        }
    }

    public static void OnMouseMove(MouseState mouse)
    {
        if (!_initialized) return;
        for (int i = components.Count - 1; i >= 0; i--)
        {
            var c = components[i];
            if (c.OnMouseMove(mouse)) break;
        }
    }

    public static void OnMouseWheel(MouseState mouse)
    {
        if (!_initialized) return;
        for (int i = components.Count - 1; i >= 0; i--)
        {
            var c = components[i];
            if (c.OnMouseWheel(mouse)) break;
        }
    }

    public static void OnUpdate(float delta, MouseState mouse, KeyboardState keys)
    {
        if (!_initialized) return;
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
            if (!components.Remove(component)) throw new InvalidOperationException("Attempted to deregister a UI element that was not registered.");
        }
        if (pendingRemovals.Count > 0) pendingRemovals.Clear();
    }

    public static Vector2 ConvertMouseScreenCoords(Vector2 position)
    {
        return new Vector2(position.X, window is not null ? (window.Size.Y - position.Y) : position.Y) * InvDPIScaleVec2;
    }

    public static void DrawElements()
    {
        renderer?.DrawInstances();
    }
}