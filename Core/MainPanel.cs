using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OTK.LiteUI.Managers;

public class MainPanel : GameWindow
{
    private static float _currentDelta = 0.0f;
    public static float DeltaTime => _currentDelta;
    private static float _totalTime = 0.0f;
    public static float TotalTime => _totalTime;
    private int FPSCount = 0;
    private float FPSTickTime = 0;
    private static float tick = 0.5f;
    public static Vector2 Dimensions = new Vector2(1280, 720);
    public static bool hitWall = false, grounded = false, atEdge = false;
    Mesh? mesh = null;
    Material? material = null;
    InstanceRenderer? renderer = null;
    Matrix4 projection;
    public static UIQuad quad;
    private static TextureResolution grassResolution = TextureResolution.R512;
    public MainPanel(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
    {
        WindowState = WindowState.Fullscreen;
        VSync = VSyncMode.On;
        Dimensions = new Vector2(Size.X, Size.Y);
        projection = Matrix4.CreateOrthographic(Dimensions.X, Dimensions.Y, 0.1f, 10.0f);
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(0.2f, 0.2f, 0.2f, 1);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Enable(EnableCap.CullFace);
        TextureManager.CreateResolution(TextureResolution.R256, 128);
        TextureManager.CreateResolution(TextureResolution.R512, 128);
        TextureManager.CreateResolution(TextureResolution.R1024, 128);
        int layer = -1;
        if (!TextureManager.TryLoadTexture("/Users/liam/VS Code Projects/OTK.LiteUI/Assets/Textures/grass.png", "Grass", out grassResolution)) Console.WriteLine("Failed To Load Texture");
        else
        {
            TextureManager.TryGetTexture("Grass", grassResolution, out layer);
        }
        Vector2 offset = new Vector2(333.0f, 250);
        Vector4 quadOffset = new Vector4(offset.X, offset.Y, offset.X, offset.Y);
        float width = 100;
        float height = 100;
        float halfHeight = 42.0f;
        float halfWidth = 333.0f;

        quad = new UIQuad() { position = new Vector2(25, 12.5f), size = new Vector2(50, 25), UVOffset = new Vector2(), UVRange = new Vector2(1, 1), colour = new Vector4(0, 1, 0, 0.5f), textureLayer = layer };
        Vector4 testBounds = new Vector4(Dimensions.X * 0.5f - width, Dimensions.Y * 0.5f - height, Dimensions.X * 0.5f + width, Dimensions.Y * 0.5f + height) + quadOffset;
        Vector4 horizontalTestBounds = new Vector4(Dimensions.X * 0.5f - halfWidth, Dimensions.Y * 0.5f - halfHeight, Dimensions.X * 0.5f + halfWidth, Dimensions.Y * 0.5f + halfHeight) + quadOffset;
        Vector4 verticalTestBounds = new Vector4(Dimensions.X * 0.5f - 10, Dimensions.Y * 0.5f - 100, Dimensions.X * 0.5f + 10, Dimensions.Y * 0.5f + 100) + quadOffset;
        mesh = Mesh.Load("/Users/liam/VS Code Projects/OTK.LiteUI/Assets/Meshes/Quad.obj");
        material = Material.Load("/Users/liam/VS Code Projects/OTK.LiteUI/Assets/Materials/UIMaterial.mat");
        InstanceAttribType[] attribTypes = [InstanceAttribType.Position2D, InstanceAttribType.Vec2, InstanceAttribType.TexCoords, InstanceAttribType.Vec2, InstanceAttribType.Color4, InstanceAttribType.Single];
        renderer = new InstanceRenderer(mesh, material, attribTypes);
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        Dimensions = new Vector2(Size.X, Size.Y);
    }

    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        base.OnKeyDown(e);
    }

    protected override void OnKeyUp(KeyboardKeyEventArgs e)
    {
        base.OnKeyUp(e);
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        base.OnMouseMove(e);
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
    }

    protected override void OnTextInput(TextInputEventArgs e)
    {
        base.OnTextInput(e);
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        float delta = (float)args.Time;
        _currentDelta = delta;
        _totalTime += delta;
        FPSTickTime += delta;
        if (FPSTickTime >= tick)
        {
            FPSTickTime -= tick;
            FPSCount = 0;
        }
        Vector2 dir = new Vector2();
        var speed = 1.0f;
        if (KeyboardState.IsKeyDown(Keys.Left)) dir.X -= speed;
        if (KeyboardState.IsKeyDown(Keys.Right)) dir.X += speed;
        if (KeyboardState.IsKeyDown(Keys.Up)) dir.Y += speed;
        if (KeyboardState.IsKeyDown(Keys.Down)) dir.Y -= speed;
        dir *= delta;
        FPSCount++;
        material?.SetMatrix4("vpMatrix", projection);
        material?.UpdateUniforms();
        renderer?.AddInstance(quad);
    }

    public override void Close()
    {
        base.Close();
    }

    private void DrawEntities()
    {

    }

    private void DrawUI()
    {
        TextureManager.Bind(grassResolution, 0);
        renderer?.DrawInstances();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        DrawEntities();
        DrawUI();

        SwapBuffers();
    }
}