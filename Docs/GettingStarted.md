# Getting Started

This guide shows how to integrate OTK.LiteUI into an OpenTK application and create a basic UI scene with interactive components.

---

## 1. Required OpenGL / Window Setup

OTK.LiteUI assumes a valid OpenGL context is active.

Typical setup inside your `GameWindow`:

```csharp
protected override void OnLoad()
{
    base.OnLoad();

    GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
    GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
    GL.Enable(EnableCap.CullFace);
}
```

Before creating any UI components, initialize the UI system with your active window:

```csharp
UIScene.Initialize(this);
```

You should also set the UI resolution mode. By default it is set to TextureResolution.R256.

```csharp
UIScene.Resolution = TextureResolution.R256;
```

OTK.LiteUI uses a Texture2DArray for batching performance.

You must create a texture array before loading textures:

```csharp
TextureManager.CreateTextureArray(128);
```

Then load textures into named slots:

```csharp
TextureManager.TryLoadTexture("Assets/Textures/Button.png", "Button", out _, UIScene.Resolution, EmptyPixelType.Transparent);

TextureManager.TryLoadTexture("Assets/Textures/CheckboxEmpty.png", "Unchecked", out _, UIScene.Resolution, EmptyPixelType.Transparent);

TextureManager.TryLoadTexture("Assets/Textures/CheckboxFilled.png", "Checked", out _, UIScene.Resolution, EmptyPixelType.Transparent);
```

These string keys are later referenced by UI components.

You must load a font before Text can be rendered on screen.

```csharp
FontManager.LoadFont("/Users/liam/VS Code Projects/OTK.LiteUI/Assets/Fonts/Roboto.ttf", charsize, resolutionX, resolutionY);
```

Once initialization is complete, UI elements can be created directly.

- Button

```csharp
var button = new Button(new Vector4(10, 30, 110, 65), "Button");
button.Texture = "Button";
button.TextColour = new Vector4(1, 0, 0, 1);
```

- Label

```csharp
var label = new Label(
    new Vector2(10, 720),
    25.0f,
    "The quick brown fox\njumped over\nthe lazy dog"
);
```

- Checkbox

```csharp
var checkbox1 = new Checkbox(new Vector4(120, 10, 170, 60));
var checkbox2 = new Checkbox(new Vector4(180, 10, 230, 60));

checkbox1.UncheckedTexture = "Unchecked";
checkbox1.CheckedTexture = "Checked";

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
```

- Slider

```csharp
var slider = new Slider(new Vector4(360, 30, 560, 60));

slider.Texture = "Unchecked";
slider.ThumbTexture = "Button";
```

- StatusBar

```csharp
var statusBar = new StatusBar(new Vector4(240, 30, 340, 55));

statusBar.FillColour = new Vector4(0, 0, 1, 1);
statusBar.FillAmount = 0.75f;

statusBar.Texture = "Unchecked";
statusBar.FillTexture = "Button";
```

- Text Input

```csharp
var textField = new TextField(new Vector4(760, 10, 1160, 55));
textField.Texture = "Unchecked";

var numberField = new NumericField(new Vector4(1170, 10, 1420, 55));
numberField.Texture = "Unchecked";
```

- Panels

```csharp
var panel = new Panel(
    new Vector4(50, 200, 450, 500),
    new HorizontalGridLayout(1, new Vector2(150, 35), 15),
    15,
    0.25f
);

panel.Texture = "Button";

panel.AddChild(spinner);
panel.AddChild(textField);
panel.AddChild(checkbox1);
panel.AddChild(checkbox2);
panel.AddChild(slider);
panel.AddChild(button);
panel.AddChild(statusBar);
```

- File Navigation

```csharp
var navigator = new FileNavigator(new Vector4(50, 250, 350, 550));

navigator.Texture = "Button";

navigator.TextFieldTexture = "Unchecked";
navigator.SubPanelTexture = "Unchecked";

navigator.ButtonTexture = "Button";
navigator.BackButtonTexture = "Button";
```

- Key Behaviours:

### Automatic Registration

All UI components are automatically registered to UIScene when constructed.

No manual registration is required.

### Event-Driven Interaction

Most interactive components expose events such as:

- OnClick
- OnValueChanged
- Immediate Usage

Once created, components are active immediately within the UI system.

No additional “add to scene” step is required.

### Rendering

Call UIScene.DrawElements() during your OnRenderFrame after your scene rendering:

```csharp
UIScene.DrawElements();
```
