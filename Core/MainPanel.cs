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
    public static Label? label = null;
    public static Button? button = null;
    public static Checkbox? checkbox1 = null;
    public static Checkbox? checkbox2 = null;
    public MainPanel(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
    {
        WindowState = WindowState.Fullscreen;
        // VSync = VSyncMode.On;
        Dimensions = new Vector2(Size.X, Size.Y);
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.ClearColor(0.2f, 0.2f, 0.2f, 1);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.Enable(EnableCap.CullFace);
        UIScene.Initialize(this, TextureResolution.R512);
        TextureManager.CreateResolution(UIScene.resolution, 128);
        if (!TextureManager.TryLoadTexture("/Users/liam/VS Code Projects/OTK.LiteUI/Assets/Textures/grass.png", "Grass", out resolution, UIScene.resolution, EmptyPixelType.Transparent)) Console.WriteLine("Failed To Load Grass Texture");
        if (!TextureManager.TryLoadTexture("/Users/liam/VS Code Projects/OTK.LiteUI/Assets/Textures/DefaultButton.png", "Button", out resolution, UIScene.resolution, EmptyPixelType.Transparent)) Console.WriteLine("Failed To Load Button Texture");
        if (!TextureManager.TryLoadTexture("/Users/liam/VS Code Projects/OTK.LiteUI/Assets/Textures/CheckboxEmpty.png", "Unchecked", out resolution, UIScene.resolution, EmptyPixelType.Transparent)) Console.WriteLine("Failed To Load Unchecked Texture");
        if (!TextureManager.TryLoadTexture("/Users/liam/VS Code Projects/OTK.LiteUI/Assets/Textures/CheckboxFilled.png", "Checked", out resolution, UIScene.resolution, EmptyPixelType.Transparent)) Console.WriteLine("Failed To Load Checked Texture");
        Vector2 offset = new Vector2(333.0f, 250);
        Vector4 quadOffset = new Vector4(offset.X, offset.Y, offset.X, offset.Y);
        float width = 100;
        float height = 100;
        float halfHeight = 42.0f;
        float halfWidth = 333.0f;

        Vector4 testBounds = new Vector4(Dimensions.X * 0.5f - width, Dimensions.Y * 0.5f - height, Dimensions.X * 0.5f + width, Dimensions.Y * 0.5f + height) + quadOffset;
        Vector4 horizontalTestBounds = new Vector4(Dimensions.X * 0.5f - halfWidth, Dimensions.Y * 0.5f - halfHeight, Dimensions.X * 0.5f + halfWidth, Dimensions.Y * 0.5f + halfHeight) + quadOffset;
        Vector4 verticalTestBounds = new Vector4(Dimensions.X * 0.5f - 10, Dimensions.Y * 0.5f - 100, Dimensions.X * 0.5f + 10, Dimensions.Y * 0.5f + 100) + quadOffset;
        FontManager.LoadFont("/Users/liam/VS Code Projects/OTK.LiteUI/Assets/Fonts/Roboto.ttf", 64, 512, 512);
        button = new Button(new Vector4(10, 10, 110, 45), "Button");
        button.Texture = "Button";
        button.TextColour = new Vector4(1, 0, 0, 1);
        button.OnClick += LeftButton => { Console.WriteLine("Clicked"); };
        button.OnHoverEnter += () => { Console.WriteLine("Entered"); };
        button.OnHoverExit += () => { Console.WriteLine("Exited"); };
        label = new Label(new Vector2(10, Dimensions.Y * UIScene.InvDPIScaleY - 35.0f), 25.0f, "stuff");
        checkbox1 = new Checkbox(new Vector4(120, 10, 170, 60));
        checkbox2 = new Checkbox(new Vector4(180, 10, 230, 60));
        checkbox1.UncheckedTexture = "Unchecked";
        checkbox1.CheckedTexture = "Checked";
        checkbox1.UncheckedColour = new Vector4(1, 0, 0, 1);
        checkbox1.CheckedColour = new Vector4(0, 1, 0, 1);
        checkbox2.UncheckedTexture = "Unchecked";
        checkbox2.CheckedTexture = "Checked";
        checkbox1.OnClick += _ =>
        {
            checkbox1.Checked = true;
            checkbox2.Checked = false;
        };

        checkbox2.OnClick += _ =>
        {
            checkbox1.Checked = false;
            checkbox2.Checked = true;
        };
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

        if (label is not null && button is not null)
        {
            label.Text = $"mouse: {UIScene.ConvertMouseScreenCoords(MouseState.Position)}, within bounds: {button.WithinBounds(MouseState)}, is pressed: {button._isPressed}";
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