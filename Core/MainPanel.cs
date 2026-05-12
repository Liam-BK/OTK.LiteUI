using System.Runtime.InteropServices;
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
    private static float count = 0;
    public static Label? label = null;
    public static Button? button = null;
    public static Checkbox? checkbox1 = null;
    public static Checkbox? checkbox2 = null;
    public static StatusBar? statusBar = null;
    public static Slider? slider = null;
    public static Slider? verticalSlider = null;
    public static ScrollBar? horizontalScrollBar = null;
    public static ScrollBar? verticalScrollBar = null;
    public static TextField? textField = null;
    public static NumericSpinner? spinner = null;
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
        UIScene.Initialize(this);
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
        button.Bounds = new Vector4(10, 30, 110, 65);
        // button.IsVisible = false;
        label = new Label(new Vector2(10, Dimensions.Y * UIScene.InvDPIScaleY - 35.0f), 25.0f, $"The quick brown fox\njumped over\nthe lazy dog");
        checkbox1 = new Checkbox(new Vector4(120, 10, 170, 60));
        checkbox2 = new Checkbox(new Vector4(180, 10, 230, 60));
        checkbox1.UncheckedTexture = "Unchecked";
        checkbox1.CheckedTexture = "Checked";
        checkbox1.UncheckedColour = new Vector4(1, 0, 0, 1);
        checkbox1.CheckedColour = new Vector4(0, 1, 0, 1);
        checkbox2.UncheckedTexture = "Unchecked";
        checkbox2.CheckedTexture = "Checked";
        // checkbox1.IsVisible = false;
        // checkbox2.IsVisible = false;
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
        statusBar = new StatusBar(new Vector4(240, 10, 340, 35));
        statusBar.FillColour = new Vector4(0, 0, 1, 1);
        statusBar.FillAmount = 0.75f;
        statusBar.Texture = "Unchecked";
        statusBar.FillTexture = "Button";
        statusBar.Bounds = new Vector4(240, 30, 340, 55);
        // statusBar.IsVisible = false;
        slider = new Slider(new Vector4(360, 10, 560, 40));
        slider.Texture = "Unchecked";
        slider.ThumbTexture = "Button";
        slider.Bounds = new Vector4(360, 30, 560, 60);
        verticalSlider = new Slider(new Vector4(570, 10, 600, 210));
        verticalSlider.Texture = "Unchecked";
        verticalSlider.ThumbTexture = "Checked";
        verticalSlider.Value = 0.5f;

        horizontalScrollBar = new ScrollBar(new Vector4(610, 10, 710, 35));
        horizontalScrollBar.Texture = "Unchecked";
        horizontalScrollBar.ThumbTexture = "Button";
        horizontalScrollBar.ContentSize = Vector2.One * 2;
        horizontalScrollBar.Value = 0.5f;
        horizontalScrollBar.Bounds = new Vector4(610, 30, 710, 55);
        verticalScrollBar = new ScrollBar(new Vector4(720, 10, 750, 210));
        verticalScrollBar.Texture = "Unchecked";
        verticalScrollBar.ThumbTexture = "Button";
        verticalScrollBar.ContentSize = Vector2.One * 2;
        verticalScrollBar.Value = 0.5f;
        verticalScrollBar.Bounds = new Vector4(720, 30, 750, 230);

        textField = new TextField(new Vector4(760, 10, 1160, 110));
        textField.Texture = "Unchecked";

        spinner = new NumericSpinner(new Vector4(1170, 10, 1420, 55));
        spinner.Texture = "Unchecked";
        spinner.ButtonTexture = "Button";
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
        count += delta;
        if (count > 2 * MathF.PI) count -= 2 * MathF.PI;
        if (statusBar is not null)
        {
            statusBar.FillAmount = MathF.Sin(count) * 0.5f + 0.5f;
        }

        if (FPSTickTime >= tick)
        {
            FPSTickTime -= tick;
            FPSCount = 0;
        }

        if (label is not null && spinner is not null)
        {
            label.Text = $"spinner value: {spinner.Value}";
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