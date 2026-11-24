# Getting Started

This guide walks through setting up OTK.LiteUI in an OpenTK application, creating a UI canvas, adding components, and rendering an interactive interface.
The guide also assumes Visual Studio Code as your IDE.

## Prerequisites

Before getting started, you'll need:

- A working C# development environment.
- A .NET project compatible with OTK.LiteUI.
- OpenTK configured in your project.
- An OpenGL-compatible environment supported by the library.

## 1. Project Setup

Create a new OpenTK project, then add OTK.LiteUI using the .NET CLI.

```bash
dotnet add package OTK.LiteUI
```

## 2. Creating an OpenTK Window

OTK.LiteUI integrates with OpenTK's `GameWindow`. Create a window that will host your UI:

```csharp
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.IO;

public static class Program
{
    public static void Main()
    {
        if (OperatingSystem.IsLinux())
        {
            GLFW.InitHint(InitHintPlatform.Platform, Platform.X11);
            GLFW.InitHint(InitHintInt.WaylandLibDecor, (int)WaylandLibDecor.DisableLibDecor);
        }
        Directory.SetCurrentDirectory(Path.Combine(AppContext.BaseDirectory, @"../../../"));
        var gameSettings = GameWindowSettings.Default;
        var nativeSettings = new NativeWindowSettings()
        {
            Title = "UI Test",
            StencilBits = 8,
            NumberOfSamples = 4
        };

        using var window = new MainPanel(gameSettings, nativeSettings);
        window.Run();
    }
}

```

## 3. Initialising the UI Canvas

The `UICanvas` manages UI components, rendering resources, input handling, and the overall UI hierarchy.

The texture array stores the textures used by UI components. Its capacity determines how many texture layers it can hold, while `TextureResolution.R512` sets the resolution of each layer.

Each loaded font generates a texture atlas that uses a layer in the texture array. The atlas contains the glyph textures used when rendering text.

Fonts must be loaded before creating components that use text.

Create a canvas when the window loads:

```csharp
UICanvas canvas;

//override the OnLoad method from OpenTK's GameWindow
protected override void OnLoad()
{
    base.OnLoad();

    // Create the UI canvas
    canvas = new UICanvas(this);

    // Configure the texture array. 
    int capacity = 64;
    canvas.CreateTextureArray(capacity, TextureResolution.R512);

    // Load a font
    string fontPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts", "Inter-Font.ttf");

    int glyphResolution = 64;
    canvas.LoadFont(fontPath, glyphResolution);
}
```

Make sure to configure the texture array and load at least one font before using components that depend on them.

## 4. Creating UI Components

Components can be created programmatically and are automatically added to the UICanvas during construction.

```csharp
//also inside OnLoad:

//height of element on the Panel
float layoutElementHeight = 25.0f;

//padding between elements on the panel
float layoutPadding = 5.0f;

//twice the size of inset
Vector4 defaultBounds = new Vector4(0, 0, 20, 20);

var panel = new Panel(new Vector4(-200, -200, 200, 200), new VerticalLayout(layoutElementHeight, layoutPadding), 10, 0.25f, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
panel.Anchor = AnchorType.Center;

var button = new Button(defaultBounds, "Hello, World!");

var label = new Label(new Vector2(0, 0), 5, "I am a Label", new Vector4(0, 0, 0, 1));
label.Alignment = TextAlignment.Center;

panel.AddChild(button);
panel.AddChild(label);
```

## 5. Handling Input

OTK.LiteUI supports mouse and keyboard interaction, including component focus and text input.

When a `UICanvas` is constructed, it automatically subscribes to the relevant OpenTK window events and handles input internally. No additional input event wiring or forwarding is required.

## 6. Updating and Rendering

The `UICanvas` automatically hooks into the `GameWindow` update events during construction, so no manual update calls are required.

Rendering, however, must be performed manually. Call `canvas.DrawElements()` during the window's render cycle to draw the UI.

```csharp
protected override void OnRenderFrame(FrameEventArgs args)
    {
        if (closing) return;
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        canvas.DrawElements();
        SwapBuffers();
    }
```

## 7. Loading UI from JSON

In addition to programmatic construction, OTK.LiteUI supports loading complete UI hierarchies from JSON definitions.

The following example defines two pages, each containing a button that navigates to the other page:

```json
[
    {
        "Type": "Page",
        "Properties": {
            "Identifier": "Page1",
            "IsVisible": false,
            "Children": [
                {
                    "Type": "Button",
                    "Properties": {
                        "Identifier": "Page1Button",
                        "Bounds": [
                            -300,
                            100,
                            -100,
                            150
                        ],
                        "Colour": [
                            0.2,
                            0.4,
                            0.8,
                            1.0
                        ],
                        "Text": "Go To Page 2",
                        "IsVisible": true,
                        "Anchor": "Center",
                        "PageLink": "Page2",
                        "HideParentPageOnClick": true
                    }
                }
            ]
        }
    },
    {
        "Type": "Page",
        "Properties": {
            "Identifier": "Page2",
            "IsVisible": true,
            "Children": [
                {
                    "Type": "Button",
                    "Properties": {
                        "Identifier": "Page2Button",
                        "Bounds": [
                            100,
                            100,
                            300,
                            150
                        ],
                        "Colour": [
                            0.2,
                            0.4,
                            0.8,
                            1.0
                        ],
                        "Text": "Go To Page 1",
                        "IsVisible": true,
                        "Anchor": "Center",
                        "PageLink": "Page1",
                        "HideParentPageOnClick": true
                    }
                }
            ]
        }
    }
]
```

This is loaded via:

```csharp
string UIPath = Path.Combine(AppContext.BaseDirectory, "Assets", "UI", "Pages.json");

using (var stream = File.OpenRead(UIPath))
{
    canvas.LoadJsonUI(stream);
}
```

## Full Example

- Program

```csharp
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;

public static class Program
{
    public static void Main()
    {
        if (OperatingSystem.IsLinux())
        {
            GLFW.InitHint(InitHintPlatform.Platform, Platform.X11);
            GLFW.InitHint(InitHintInt.WaylandLibDecor, (int)WaylandLibDecor.DisableLibDecor);
        }
        Directory.SetCurrentDirectory(Path.Combine(AppContext.BaseDirectory, @"../../../"));
        var gameSettings = GameWindowSettings.Default;
        var nativeSettings = new NativeWindowSettings()
        {
            Title = "UI Test",
            StencilBits = 8,
            NumberOfSamples = 4
        };

        using var window = new MainPanel(gameSettings, nativeSettings);
        window.Run();
    }
}
```

- MainPanel

```csharp
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OTK.LiteUI.Core;
using OTK.LiteUI.Layouts;
using OTK.LiteUI.UI.Components;

public class MainPanel : GameWindow
{
    public static bool closing = false;
    private UICanvas canvas = null!;

    public MainPanel(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings) : base(gameWindowSettings, nativeWindowSettings)
    {
        WindowState = WindowState.Maximized;
    }

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.Viewport(0, 0, FramebufferSize.X, FramebufferSize.Y);
        GL.ClearColor(0.2f, 0.2f, 0.2f, 1);
        canvas = new UICanvas(this);
        canvas.CreateTextureArray(64, TextureResolution.R512);

        string fontPath = Path.Combine(AppContext.BaseDirectory, "Assets", "Fonts", "Inter-Font.ttf");

        canvas.LoadFont(fontPath, 64);

        string UIPath = Path.Combine(AppContext.BaseDirectory, "Assets", "UI", "Pages.json");

        using (var stream = File.OpenRead(UIPath))
        {
            canvas.LoadJsonUI(stream);
        }

        //Or it can be constructed this way:
        //
        // //height of element on the Panel
        // float layoutElementHeight = 25.0f;
        // //padding between elements on the panel
        // float layoutPadding = 5.0f;
        // //twice the size of inset
        // Vector4 defaultBounds = new Vector4(0, 0, 20, 20);

        // var panel = new Panel(new Vector4(-200, -200, 200, 200), new VerticalLayout(layoutElementHeight, layoutPadding), 10, 0.25f, new Vector4(0.5f, 0.5f, 0.5f, 1.0f));
        // panel.Anchor = AnchorType.Center;
        // var button = new Button(defaultBounds, "Hello, World!");
        // var label = new Label(new Vector2(0, 0), 5, "I am a Label", new Vector4(0, 0, 0, 1));
        // label.Alignment = TextAlignment.Center;

        // panel.AddChild(button);
        // panel.AddChild(label);
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        if (closing) return;
        base.OnRenderFrame(args);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        canvas.DrawElements();
        SwapBuffers();
    }
}
```

## Components List

- Button            A clickable button that supports text and interaction events.
- Label	            Displays text within the UI.
- Checkbox	        A toggleable checkbox component.
- TextField	        Allows users to enter and edit text.
- NumericField	    Allows users to enter numeric values.
- Slider	        Allows users to select a value within a range.
- Panel	A           container for grouping and organising UI components.
- ScrollBar	        Provides scrolling functionality for scrollable content.
- StatusBar	        Displays status information.
- Page	            A container for organising UI elements into navigable pages.
- FileNavigator	    Provides file navigation functionality.
- NineSlice	        A resizable UI element rendered using nine-slice texturing.
- Blank	            A non-interactive UI element intended for creating spaces within Layouts.

## Layouts List

- FlowLayout                            Arranges visible components horizontally, wrapping them onto subsequent rows when the available width is exceeded. Uses fixed element dimensions and configurable padding.

- GridLayout                            Arranges visible components in a fixed grid of rows and columns, distributing available space evenly between cells with configurable padding.

- HorizontalGridLayout                  Arranges visible components in columns, filling each column from top to bottom before moving to the next. Uses a fixed element width and distributes available height evenly across the configured rows, with configurable padding.

- HorizontalLayout                      Arranges visible components horizontally in a single row, using fixed element dimensions and configurable padding between components.

- HorizontalSplitLayoutAbsolute         Arranges visible components horizontally using explicitly specified widths. Widths are supplied through the Splits parameter and cycle through the array when there are more components than split values. Components span the full viewport height, with configurable padding between them.

- HorizontalSplitLayoutAbsoluteFirst    Arranges components horizontally, assigning the first component a fixed width and distributing the remaining available width proportionally among subsequent components using the Splits array. Components span the full viewport height, with configurable padding between them.

- HorizontalSplitLayoutAbsoluteLast     Arranges components horizontally, distributing the available width proportionally among preceding components using the Splits array and assigning the last component a fixed width. Components span the full viewport height, with configurable padding between them.

- HorizontalSplitLayoutProportional     Arranges components horizontally, assigning each a proportion of the available width using the Splits array. Split values cycle through the array as necessary. Components span the full viewport height, with configurable padding between them.

- HorizontalSplitLayoutSandwich         Arranges components horizontally, assigning fixed widths to the first and last components and distributing the remaining available width proportionally among the components in between using the Splits array. Split values cycle through the array as necessary. Components span the full viewport height, with configurable padding between them.

- HorizontalSquareLayout                Arranges visible components in columns, filling each column from top to bottom before moving to the next. Cells are square, with their dimensions determined by the available viewport height and configured row count. Uses configurable padding between cells.

- VerticalGridLayout                    Arranges visible components in rows, filling each row from left to right before moving to the next. The number of columns is configurable, with available width distributed evenly between them. Uses a fixed element height and configurable padding.

- VerticalLayout                        Arranges visible components vertically in a single column, from top to bottom. Components stretch to the available viewport width and use a fixed height, with configurable padding between them.

- VerticalSplitLayoutAbsolute           Arranges visible components vertically using explicitly specified heights from the Splits array, cycling through the values as necessary. Components stretch to the full viewport width, with configurable padding between them.

- VerticalSplitLayoutAbsoluteFirst      Arranges components vertically, assigning the first component a fixed height and distributing the remaining available height proportionally among subsequent components using the Splits array. Split values cycle through the array as necessary. Components stretch to the full viewport width, with configurable padding between them.

- VerticalSplitLayoutAbsoluteLast       Arranges components vertically, distributing the available height proportionally among preceding components using the Splits array and assigning the last component a fixed height. Split values cycle through the array as necessary. Components stretch to the full viewport width, with configurable padding between them.

- VerticalSplitLayoutProportional       Arranges components vertically, assigning each a proportion of the available height using the Splits array. Split values cycle through the array as necessary. Components stretch to the full viewport width, with configurable padding between them.

- VerticalSplitLayoutSandwich           Arranges components vertically, assigning fixed heights to the first and last components. The available space between these components is distributed proportionally among the intermediate components using the Splits array, with values cycling as necessary. Components stretch to the full viewport width, with configurable padding between them.

- VerticalSquareLayout                  Arranges visible components in rows, filling each row from left to right before moving to the next. Cells are square, with their dimensions determined by the available viewport width and configured column count. Uses configurable padding between cells.

## Next Steps

Explore the API documentation for details about individual components, layouts, rendering options, and other features.