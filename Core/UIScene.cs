using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;

public static class UIScene
{
    private static bool _initialized = false;
    public static bool Initialized => _initialized;
    private static InstanceRenderer? renderer;
    private static Material? material;
    public static Matrix4 projection;
    public static List<IUIElement> components = new List<IUIElement>();
    public static void Initialize(Matrix4 viewProjection)
    {
        if (_initialized) return;
        var mesh = Mesh.Load("/Users/liam/VS Code Projects/OTK.LiteUI/Assets/Meshes/Quad.obj");
        material = Material.Load("/Users/liam/VS Code Projects/OTK.LiteUI/Assets/Materials/UIMaterial.mat");
        InstanceAttribType[] attribTypes = [InstanceAttribType.Position2D, InstanceAttribType.Vec2, InstanceAttribType.TexCoords, InstanceAttribType.Vec2, InstanceAttribType.Color4, InstanceAttribType.Single];
        renderer = new InstanceRenderer(mesh, material, attribTypes);
        projection = viewProjection;
        _initialized = true;
    }

    public static void Register(IUIElement component)
    {
        if (!_initialized) return;
        components.Add(component);
    }

    public static void OnClickDown(MouseState mouse)
    {
        if (!_initialized) return;
        for (int i = components.Count - 1; i >= 0; i--)
        {
            if (components[i].OnClickDown(mouse)) break;
        }
    }

    public static void OnClickUp(MouseState mouse)
    {
        if (!_initialized) return;
        for (int i = components.Count - 1; i >= 0; i--)
        {
            if (components[i].OnClickUp(mouse)) break;
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
        for (int i = components.Count - 1; i >= 0; i--)
        {
            components[i].OnTextInput(args);
        }
    }

    public static void OnMouseMove(MouseState mouse)
    {
        if (!_initialized) return;
        for (int i = components.Count - 1; i >= 0; i--)
        {
            if (components[i].OnMouseMove(mouse)) break;
        }
    }

    public static void OnMouseWheel(MouseState mouse)
    {
        if (!_initialized) return;
        for (int i = components.Count - 1; i >= 0; i--)
        {
            if (components[i].OnMouseWheel(mouse)) break;
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
    }

    public static void DrawElements()
    {
        renderer?.DrawInstances();
    }
}