# OTK.LiteUI

A batched UI library for OpenTK designed for responsive tooling, low rendering overhead, and straightforward integration.

It is intended primarily for in-engine editors, tools, and game UI systems where predictable performance and simple control flow are preferred over highly abstract UI frameworks.

---

## Features

- Batched UI rendering
- Single-pass quad submission
- Minimal GPU state changes
- Lightweight clipping system
- Panels and layout containers
- Buttons
- Text fields
- Scrollable containers
- Keyboard and mouse input handling
- OpenTK integration

---

## Design Goals

OTK.LiteUI is built around the following principles:

- predictable performance under high UI density
- low abstraction overhead
- explicit control flow
- fast iteration for tooling workflows
- minimal renderer complexity

The library is designed for practical UI construction in tooling contexts rather than general-purpose retained-mode UI systems.

---

## Quick Start

```csharp
UIScene.Initialize(gameWindow);

var button = new Button(
    new Vector4(100, 100, 250, 140),
    "Click Me"
);

button.OnClick += _ =>
{
    Console.WriteLine("Button clicked.");
};
```

UI components are automatically registered to the active UIScene upon creation.

## Core Concepts

### UI Components

- UI elements are represented as components such as Button, TextField, and Panel.

Each component:

- stores its own state
- handles input events
- generates renderable quads each frame

UI components are evaluated each frame into geometry rather than being retained as GPU-side render objects.

### UIScene

UIScene is the global container for all active UI components.

It is responsible for:

- updating all components each frame
- dispatching input events
- coordinating rendering submission

All components are automatically registered to the active scene upon construction.

### Input Handling

Input is processed each frame and dispatched to UI components based on:

- mouse position
- focus state
- interaction bounds

Only the first valid component consumes an input event.

### Layout Model

The UI system does not include an automatic layout engine.

All UI positioning is defined explicitly using Vector4 bounds:

- X = left
- Y = top
- Z = right
- W = bottom

### State Model

The system uses a hybrid state model:

- UI logic is stateful (components retain state)
- rendering is stateless (UI is rebuilt into quads each frame)

This ensures predictable rendering performance regardless of UI complexity.

### Rendering Model

The renderer uses a flattened quad submission pipeline.

UI elements generate UIQuad structures which are submitted to an InstanceRenderer for batched rendering.

This pipeline is designed to minimize:

- draw calls
- shader switches
- GPU state changes

Each quad contains:

- position
- size
- UV region
- texture layer
- color tint
- Texture System

The renderer uses a Texture2DArray to enable efficient batching and reduce texture binding overhead.

All textures must conform to a fixed resolution within the texture array.

This is a structural requirement of the rendering system.

If textures do not conform to this constraint, visual artifacts may occur, including:

- UV misalignment
- texture bleeding
- distorted sampling
- incorrect sprite placement

These issues arise from enforcing fixed-size texture layers to support single-pass batched rendering.

### Clipping

Clipping is handled using a lightweight rectangular bounds check during quad submission.

This avoids GPU clipping overhead and keeps rendering stateless at the submission level.

### Constraints / Known Limitations

#### Texture System

The UI renderer requires all textures to conform to a fixed size due to the use of a Texture2DArray.

Non-conforming textures may result in rendering artifacts as described in the Rendering Model section.

Layout System

There is no automatic layout engine. Child elements of Panel objects do have a layout applied to them.

All UI elements must be positioned manually using explicit bounds.

#### Rendering Model

UI is rebuilt into quads each frame and submitted to a batched renderer.

There is no retained GPU scene graph; all rendering is derived from CPU-side UI state.

#### Platform Dependency

This library depends on OpenTK and assumes:

- an active OpenGL context
- a GameWindow-driven lifecycle
- a consistent frame update loop
- Example Components

Built-in components include:

- Button
- TextField
- Panel
- NineSlice
- FileNavigator
- Current Status

This library is actively used for internal tooling and editor development.

Current focus:

- documentation refinement
- layout and usability improvements
- expansion of editor-oriented components

### License

MIT License
