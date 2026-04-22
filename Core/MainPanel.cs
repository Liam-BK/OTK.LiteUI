using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

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
    public static TextureResolution resolution = TextureResolution.R256;
    public MainPanel(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
    {
        WindowState = WindowState.Fullscreen;
        VSync = VSyncMode.On;
        Dimensions = new Vector2(Size.X, Size.Y);
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(0.2f, 0.2f, 0.2f, 1);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Enable(EnableCap.CullFace);
        TextureManager.CreateResolution(TextureResolution.R256, 128);
        if (!TextureManager.TryLoadTexture("/Users/liam/VS Code Projects/OTK.LiteUI/Assets/Textures/grass.png", "Grass", out resolution, TextureResolution.R256, EmptyPixelType.Transparent)) Console.WriteLine("Failed To Load Grass Texture");
        if (!TextureManager.TryLoadTexture("/Users/liam/VS Code Projects/OTK.LiteUI/Assets/Textures/DefaultButton.png", "Button", out resolution, TextureResolution.R256, EmptyPixelType.Transparent)) Console.WriteLine("Failed To Load Button Texture");
        Vector2 offset = new Vector2(333.0f, 250);
        Vector4 quadOffset = new Vector4(offset.X, offset.Y, offset.X, offset.Y);
        float width = 100;
        float height = 100;
        float halfHeight = 42.0f;
        float halfWidth = 333.0f;

        Vector4 testBounds = new Vector4(Dimensions.X * 0.5f - width, Dimensions.Y * 0.5f - height, Dimensions.X * 0.5f + width, Dimensions.Y * 0.5f + height) + quadOffset;
        Vector4 horizontalTestBounds = new Vector4(Dimensions.X * 0.5f - halfWidth, Dimensions.Y * 0.5f - halfHeight, Dimensions.X * 0.5f + halfWidth, Dimensions.Y * 0.5f + halfHeight) + quadOffset;
        Vector4 verticalTestBounds = new Vector4(Dimensions.X * 0.5f - 10, Dimensions.Y * 0.5f - 100, Dimensions.X * 0.5f + 10, Dimensions.Y * 0.5f + 100) + quadOffset;
        UIScene.Initialize(this, TextureResolution.R256);
        FontManager.LoadFont("/Users/liam/VS Code Projects/OTK.LiteUI/Assets/Fonts/DejaVuSans.ttf", 32, 512, 512);
        var nineSlice = new NineSlice(new Vector4(-200, -200, 200, 200), 50, 0.125f, new Vector4(0, 1, 0, 1));
        nineSlice.Texture = "DejaVuSans";
        var label = new Label(new Vector2(), 50, "Hello");
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);
        Dimensions = new Vector2(Size.X, Size.Y);
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

        FPSCount++;
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
        UIScene.DrawElements();
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