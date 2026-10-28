using System.Diagnostics;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OTK.LiteUI.Core;
using OTK.LiteUI.Layouts;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.UI.Rendering;

namespace OTK.LiteUI.UI.Components
{
    public class FileNavigator : UIComponent, IRenderable
    {
        Panel panel;

        public override AnchorType Anchor
        {
            get => base.Anchor;
            set
            {
                base.Anchor = value;
                panel.Anchor = value;
            }
        }

        public override Vector4 Colour
        {
            get => base.Colour;
            set
            {
                base.Colour = value;
                panel.Colour = value;
            }
        }

        public char Delimiter = OperatingSystem.IsWindows() ? '\\' : '/';

        private Button Confirm, Cancel, Back;

        private TextField Search, NameInput;

        private Panel QuickAccess, CurrentDirectory;

        private Label totalItems;

        private int SelectedFileCount
        {
            get
            {
                int result = 0;
                foreach (var element in CurrentDirectory.Children)
                {
                    if (element is FileReference fileRef && fileRef.selected)
                    {
                        result++;
                    }
                }
                return result;
            }
        }

        private int ItemCount
        {
            get => CurrentDirectory.Children.Count;
        }

        private readonly Vector4 directoryColour = new Vector4(1.0f, 0.9f, 0.5f, 1.0f);

        private readonly Vector4 fileColour = new Vector4(0.55f, 0.85f, 1.0f, 1.0f);

        public string CurrentPath
        {
            get;
            set;
        } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        public string TextFieldTexture
        {
            set
            {
                if (Search is not null) Search.Texture = value;
                if (NameInput is not null) NameInput.Texture = value;
            }
        }

        public string SubPanelTexture
        {
            set
            {
                if (QuickAccess is not null) QuickAccess.Texture = value;
                if (CurrentDirectory is not null) CurrentDirectory.Texture = value;
            }
        }

        public string ButtonTexture
        {
            set
            {
                if (Confirm is not null) Confirm.Texture = value;
                if (Cancel is not null) Cancel.Texture = value;
            }
        }

        public override Vector4 Bounds
        {
            get => panel.Bounds;
            set
            {
                base.Bounds = value;
                panel.Bounds = value;
            }
        }

        private bool _isInitialized = false;

        private bool _refreshPending = false;

        public static bool multiSelect = false;

        public override bool IsVisible
        {
            get => base.IsVisible;
            set
            {
                base.IsVisible = value;
                panel.IsVisible = value;
            }
        }

        public FileNavigator(UICanvas canvas, Vector4 bounds)
        {
            Canvas = canvas;
            var defaultBounds = new Vector4(0, 0, 20, 20);
            float padding = 10;
            panel = new Panel(canvas, bounds, new VerticalSplitLayoutSandwich(padding, 25, 25, 1));
            var topPanelLayer1 = new Panel(canvas, defaultBounds, new HorizontalSplitLayoutAbsoluteFirst(padding, 50, 1), 0, 0);
            topPanelLayer1.Colour = Vector4.Zero;
            panel.AddChild(topPanelLayer1);
            var middlePanelLayer1 = new Panel(canvas, defaultBounds, new HorizontalSplitLayoutAbsoluteFirst(padding, 100, 1), 0, 0);
            middlePanelLayer1.Colour = Vector4.Zero;
            panel.AddChild(middlePanelLayer1);
            var bottomPanelLayer1 = new Panel(canvas, defaultBounds, new HorizontalSplitLayoutAbsoluteLast(padding, 130, 1), 0, 0);
            bottomPanelLayer1.Colour = Vector4.Zero;
            panel.AddChild(bottomPanelLayer1);
            QuickAccess = new Panel(canvas, defaultBounds, new VerticalLayout(new Vector2(0, 25), 0), 0, 0);
            // QuickAccess.Colour = new Vector4(1, 0, 0, 1);
            QuickAccess.Colour = Vector4.One;
            CurrentDirectory = new Panel(canvas, defaultBounds, new VerticalLayout(new Vector2(0, 25), 0), 0, 0);
            // // CurrentDirectory.Colour = new Vector4(0, 1, 0, 1);
            CurrentDirectory.Colour = Vector4.One;
            middlePanelLayer1.AddChild(QuickAccess);
            middlePanelLayer1.AddChild(CurrentDirectory);
            totalItems = new Label(canvas, new Vector2(), 5);
            totalItems.Colour = new Vector4(0, 0, 0, 1);
            Confirm = new Button(canvas, defaultBounds, "Confirm");
            Cancel = new Button(canvas, defaultBounds, "Cancel");
            Back = new Button(canvas, defaultBounds, "Back");
            Back.OnClick += _ =>
            {
                NavigateUp();
                _refreshPending = true;
            };
            Search = new TextField(canvas, defaultBounds, padding * 0.35f);
            Search.AutoScaleTextByBounds = true;
            Search.OnTextChanged += RefreshCurrentFolder;
            NameInput = new TextField(canvas, defaultBounds, padding * 0.35f);
            NameInput.AutoScaleTextByBounds = true;
            topPanelLayer1.AddChild(Back);
            topPanelLayer1.AddChild(Search);
            bottomPanelLayer1.AddChild(NameInput);
            var confirmCancelPanel = new Panel(canvas, defaultBounds, new GridLayout(1, 2, padding), 0);
            confirmCancelPanel.Colour = Vector4.Zero;
            bottomPanelLayer1.AddChild(confirmCancelPanel);
            confirmCancelPanel.AddChild(Cancel);
            confirmCancelPanel.AddChild(Confirm);
            topPanelLayer1.Identifier = "top_panel_1";
            middlePanelLayer1.Identifier = "middle_panel_1";
            bottomPanelLayer1.Identifier = "bottom_panel_1";
            confirmCancelPanel.Identifier = "confirm/cancel";
            Back.Identifier = "Back";
            Search.Identifier = "Search";
            NameInput.Identifier = "FileName";
            Cancel.Identifier = "Cancel";
            Confirm.Identifier = "Confirm";
            QuickAccess.Identifier = "QuickAccess";
            CurrentDirectory.Identifier = "CurrentFolder";
            canvas.Deregister(panel);
            if (GetType() == typeof(FileNavigator)) Identifier = Canvas.GetNextIdentifier(typeof(FileNavigator));
            var desktop = FileReference.SetUpFileRef(canvas, FileReference.Directories.Desktop);
            var documents = FileReference.SetUpFileRef(canvas, FileReference.Directories.Documents);
            var downloads = FileReference.SetUpFileRef(canvas, FileReference.Directories.Downloads);
            var music = FileReference.SetUpFileRef(canvas, FileReference.Directories.Music);
            var pictures = FileReference.SetUpFileRef(canvas, FileReference.Directories.Pictures);
            var videos = FileReference.SetUpFileRef(canvas, FileReference.Directories.Videos);
            BindQuickAccess(desktop);
            BindQuickAccess(documents);
            BindQuickAccess(downloads);
            BindQuickAccess(music);
            BindQuickAccess(pictures);
            BindQuickAccess(videos);
            QuickAccess.AddChild(null);
            QuickAccess.AddChild(totalItems);
            _isInitialized = true;
            RefreshCurrentFolder();
        }

        private void BindQuickAccess(FileReference reference)
        {
            QuickAccess.AddChild(reference);
            reference.OnClick += _ =>
            {
                CurrentPath = reference.ReferencePath;
                reference.selected = true;
                _refreshPending = true;
            };
        }

        public void NavigateUp()
        {
            if (string.IsNullOrEmpty(CurrentPath))
                return;

            string trimmed = CurrentPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            int lastSeparator = trimmed.LastIndexOfAny([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar]);

            if (lastSeparator <= 0)
                return;

            CurrentPath = trimmed[..lastSeparator];
        }

        private void RefreshCurrentFolder()
        {
            if (!Search.IsFocused) Search.Text = "";
            var filter = Search.Text.Trim();
            CurrentDirectory.Clear();
            CurrentDirectory.ScrollOffset = Vector2.Zero;
            try
            {
                var files = Directory.GetFiles(CurrentPath);
                var directories = Directory.GetDirectories(CurrentPath);
                if (directories is not null)
                {
                    Array.Sort(directories);
                    foreach (var directory in directories)
                    {
                        if (!string.IsNullOrEmpty(filter) && !Path.GetFileName(directory).Contains(filter, StringComparison.OrdinalIgnoreCase))
                            continue;
                        var directoryRef = FileReference.SetUpFileRef(Canvas, directory);
                        directoryRef.IsVisible = IsVisible;
                        directoryRef.Colour = directoryColour;
                        CurrentDirectory.AddChild(directoryRef);
                        directoryRef.DoubleClick += MouseButton =>
                        {
                            CurrentPath = directoryRef.ReferencePath;
                            _refreshPending = true;
                        };
                        directoryRef.OnClick += MouseButton =>
                        {
                            if (!multiSelect) ClearSelected();
                            directoryRef.selected = true;
                        };
                    }
                }
                if (files is not null)
                {
                    Array.Sort(files);
                    foreach (var file in files)
                    {
                        if (!string.IsNullOrEmpty(filter) && !Path.GetFileName(file).Contains(filter, StringComparison.OrdinalIgnoreCase))
                            continue;
                        var fileRef = FileReference.SetUpFileRef(Canvas, file);
                        fileRef.IsVisible = IsVisible;
                        fileRef.Colour = fileColour;
                        CurrentDirectory.AddChild(fileRef);

                        fileRef.OnClick += MouseButton =>
                        {
                            if (!multiSelect) ClearSelected();
                            var selectedCount = SelectedFileCount;
                            fileRef.selected = true;
                            if (selectedCount >= 1) NameInput.Text = $"{selectedCount + 1} Files Selected.";
                            else NameInput.Text = fileRef.Text;
                        };
                    }
                }
                SetQuickAccessSelection();
                var numOfItems = ItemCount;
                totalItems.Text = $"{numOfItems} items";
                totalItems.Colour = numOfItems == 0 ? new Vector4(1, 0, 0, 1) : new Vector4(0, 0.6f, 0, 1);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine($"User does not have access to this file location: {e.Message}");
            }
        }

        private void ClearSelected()
        {
            foreach (var element in CurrentDirectory.Children)
            {
                if (element is FileReference fr)
                {
                    fr.selected = false;
                }
            }
        }

        private void SetQuickAccessSelection()
        {
            foreach (var element in QuickAccess.Children)
            {
                if (element is FileReference fr)
                {
                    fr.selected = fr.ReferencePath.Trim(Delimiter) == CurrentPath.Trim(Delimiter);
                }
            }
        }

        private string[] GetSelectedPaths()
        {
            var list = new List<string>();
            var selectedCount = SelectedFileCount;
            if (selectedCount == 0 && NameInput.Text.Length > 0)
            {
                list.Add($"{CurrentPath}{Delimiter}{NameInput.Text}");
                return [.. list];
            }
            foreach (var element in CurrentDirectory.Children)
            {
                if (element is FileReference fr && fr.selected)
                {
                    list.Add(fr.ReferencePath);
                }
            }
            return [.. list];
        }

        private Task<string[]?> GetPathFromPicker()
        {
            var tcs = new TaskCompletionSource<string[]?>();

            IsVisible = true;

            void CancelHandler(MouseButton btn)
            {
                tcs.TrySetResult(null);
                Cleanup();
                IsVisible = false;
            }

            void ConfirmHandler(MouseButton btn)
            {
                tcs.TrySetResult(GetSelectedPaths());
                Cleanup();
                IsVisible = false;
            }

            void Cleanup()
            {
                Cancel.OnClick -= CancelHandler;
                Confirm.OnClick -= ConfirmHandler;
            }

            Cancel.OnClick += CancelHandler;
            Confirm.OnClick += ConfirmHandler;

            return tcs.Task;
        }

        public async Task<string[]?> SelectFile()
        {
            return await GetPathFromPicker();
        }

        public override bool OnClickDown(MouseState mouse)
        {
            if (!IsVisible || !WithinBounds(mouse))
            {
                UICanvas.FocusedComponent = null;
                ClearSelected();
                return false;
            }
            return panel.OnClickDown(mouse);
        }

        public override bool OnClickUp(MouseState mouse)
        {
            if (!IsVisible || !WithinBounds(mouse)) return false;
            if (multiSelect && CurrentDirectory.WithinBounds(mouse))
            {
                foreach (var child in CurrentDirectory.Children)
                {
                    if (child is FileReference fileRef && !fileRef.selected)
                    {
                        child.OnClickUp(mouse);
                    }
                }
                return true;
            }
            return panel.OnClickUp(mouse);
        }

        public override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            if (!IsVisible) return;

            if (!Search.IsFocused && !NameInput.IsFocused && !multiSelect)
            {
                if (OperatingSystem.IsMacOS())
                {
                    if (e.Key == Keys.LeftSuper || e.Key == Keys.RightSuper) multiSelect = true;
                }
                else
                {
                    if (e.Key == Keys.LeftControl || e.Key == Keys.RightControl) multiSelect = true;
                }
            }

            panel.OnKeyDown(e);
        }

        public override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            if (!IsVisible) return;

            if (OperatingSystem.IsMacOS())
            {
                if (e.Key == Keys.LeftSuper || e.Key == Keys.RightSuper) multiSelect = false;
            }
            else
            {
                if (e.Key == Keys.LeftControl || e.Key == Keys.RightControl) multiSelect = false;
            }

            panel.OnKeyUp(e);
        }


        public override bool OnMouseMove(MouseState mouse)
        {
            if (!IsVisible || !WithinBounds(mouse)) return false;
            return panel.OnMouseMove(mouse);
        }

        public override bool OnMouseWheel(MouseState mouse)
        {
            if (!IsVisible || !WithinBounds(mouse)) return false;
            return panel.OnMouseWheel(mouse);
        }

        public override void OnTextInput(TextInputEventArgs e)
        {
            if (!IsVisible) return;
            panel.OnTextInput(e);
        }

        public override void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
        {
            if (!IsVisible) return;
            if (_refreshPending)
            {
                RefreshCurrentFolder();
                _refreshPending = false;
            }
            panel.OnUpdate(deltaTime, mouse, keyboard);
        }

        public void SubmitData(Renderer renderer)
        {
            if (!IsVisible) return;
            panel.SubmitData(renderer);
        }
    }

    internal class FileReference : Button
    {
        public enum Directories
        {
            Recent,
            Desktop,
            Documents,
            Downloads,
            Music,
            Pictures,
            Videos,
        }

        public string ReferencePath
        {
            get;
            set;
        } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        private static readonly double DoubleClickThreshold = 300; // milliseconds

        private readonly Stopwatch clickTimer = new Stopwatch();

        private bool clickPending = false;

        public event Action<MouseButton>? DoubleClick;

        public bool selected = false;

        public Vector4 SelectionColor = new Vector4(0.2f, 0.6f, 1.0f, 1);

        public override Vector4 Bounds
        {
            get => base.Bounds;
            set
            {
                base.Bounds = value;
                if (label is not null)
                {
                    label.Origin = new Vector2(Bounds.X + Inset, label.Origin.Y);
                    label.ClipBounds = value;
                }
            }
        }

        public FileReference(UICanvas canvas, Vector4 bounds, string text = "", float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(canvas, bounds, text, inset, uvInset, colour)
        {
            this.Canvas = canvas;
            label.Alignment = TextAlignment.Left;
            label.Origin = new Vector2(Bounds.X + Inset, label.Origin.Y);
            label.ClipBounds = bounds;
            if (GetType() == typeof(FileReference)) Identifier = Canvas.GetNextIdentifier(typeof(FileReference));
        }

        public override bool OnClickDown(MouseState mouse)
        {
            if (!IsVisible) return false;
            if (WithinBounds(mouse))
            {
                if (clickPending && clickTimer.ElapsedMilliseconds <= DoubleClickThreshold)
                {
                    clickPending = false;
                    clickTimer.Reset();
                    DoubleClick?.Invoke(MouseButton.Left);
                }
                else
                {
                    clickPending = true;
                    clickTimer.Restart();
                }
                isPressed = true;
                return true;
            }
            return false;
        }

        public override void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
        {
            base.OnUpdate(deltaTime, mouse, keyboard);
            if (selected) ForceUpdateQuadrantColours(SelectionColor);
            if (clickPending == true && clickTimer.ElapsedMilliseconds > DoubleClickThreshold)
            {
                clickTimer.Reset();
                clickPending = false;
            }
        }

        public bool IsDirectory
        {
            get => Directory.Exists(ReferencePath);
        }

        public bool IsFile
        {
            get => File.Exists(ReferencePath);
        }

        public static FileReference SetUpFileRef(UICanvas canvas, Directories directory)
        {
            FileReference temp = new FileReference(canvas, new Vector4());
            temp.SetPath(directory);
            temp.Text = GetNameOfPath(temp.ReferencePath);
            temp.TimeToRollover = 0.1f;
            return temp;
        }

        public static FileReference SetUpFileRef(UICanvas canvas, string path)
        {
            FileReference temp = new FileReference(canvas, new Vector4());
            temp.ReferencePath = path;
            temp.Text = GetNameOfPath(temp.ReferencePath);
            temp.TimeToRollover = 0.1f;
            return temp;
        }

        private static string GetNameOfPath(string path)
        {
            if (Directory.Exists(path))
            {
                var trimmed = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
                return Path.GetFileName(trimmed);
            }
            else if (File.Exists(path))
            {
                return Path.GetFileName(path);
            }
            else
            {
                Console.WriteLine($"not a directory or file: {path}");
                return "";
            }
        }

        public void SetPath(Directories directory)
        {
            switch (directory)
            {
                case Directories.Recent:
                    ReferencePath = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
                    break;
                case Directories.Desktop:
                    ReferencePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
                    break;
                case Directories.Documents:
                    ReferencePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    break;
                case Directories.Downloads:
                    ReferencePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                    break;
                case Directories.Music:
                    ReferencePath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
                    break;
                case Directories.Pictures:
                    ReferencePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                    break;
                case Directories.Videos:
                    ReferencePath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
                    break;
            }
        }
    }
}

//*********************************************************************************************************************************************************

// using System.Diagnostics;
// using OpenTK.Mathematics;
// using OpenTK.Windowing.Common;
// using OpenTK.Windowing.GraphicsLibraryFramework;
// using OTK.LiteUI.UI.Interfaces;
// using OTK.LiteUI.Core;
// using OTK.LiteUI.Layouts;
// using OTK.LiteUI.UI.Rendering;

// namespace OTK.LiteUI.UI.Components
// {
//     public class FileNavigator : NineSlice
//     {
//         private readonly List<UIComponent> _children = new List<UIComponent>();

//         public List<UIComponent> Children => _children;
//         public char Delimiter = OperatingSystem.IsWindows() ? '\\' : '/';

//         private Button Confirm, Cancel, Back;

//         private TextField Search, NameInput;

//         private Panel QuickAccess, CurrentDirectory;

//         public Vector4 ViewPort => new(Bounds.X + Inset, Bounds.Y + Inset, Bounds.Z - Inset, Bounds.W - Inset);

//         private const float textFieldWidth = 190.0f;
//         private const float buttonWidth = 90.0f;
//         private const float padding = 10.0f;
//         private const float componentHeight = 35.0f;

//         private int SelectedFileCount
//         {
//             get
//             {
//                 int result = 0;
//                 foreach (var element in CurrentDirectory.Children)
//                 {
//                     if (element is FileReference fileRef && fileRef.selected)
//                     {
//                         result++;
//                     }
//                 }
//                 return result;
//             }
//         }

//         private readonly Vector4 directoryColour = new Vector4(1.0f, 0.9f, 0.5f, 1.0f);

//         private readonly Vector4 fileColour = new Vector4(0.55f, 0.85f, 1.0f, 1.0f);

//         public string CurrentPath
//         {
//             get;
//             set;
//         } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

//         public string TextFieldTexture
//         {
//             set
//             {
//                 if (Search is not null) Search.Texture = value;
//                 if (NameInput is not null) NameInput.Texture = value;
//             }
//         }

//         public string SubPanelTexture
//         {
//             set
//             {
//                 if (QuickAccess is not null) QuickAccess.Texture = value;
//                 if (CurrentDirectory is not null) CurrentDirectory.Texture = value;
//             }
//         }

//         public string ButtonTexture
//         {
//             set
//             {
//                 if (Confirm is not null) Confirm.Texture = value;
//                 if (Cancel is not null) Cancel.Texture = value;
//             }
//         }

//         public ILayout CurrentDirectoryLayout
//         {
//             set
//             {
//                 CurrentDirectory.Layout = value;
//             }
//         }

//         public string BackButtonTexture
//         {
//             set
//             {
//                 if (Back is not null) Back.Texture = value;
//             }
//         }

//         private Vector2 MinimumSize => new Vector2(410, 300);

//         public override Vector4 Bounds
//         {
//             get => base.Bounds;
//             set
//             {
//                 if (value.Z - value.X < MinimumSize.X)
//                 {
//                     var halfDiff = (MinimumSize.X - (value.Z - value.X)) * 0.5f;
//                     value.X -= halfDiff;
//                     value.Z += halfDiff;
//                 }
//                 if (value.W - value.Y < MinimumSize.Y)
//                 {
//                     var halfDiff = (MinimumSize.Y - (value.W - value.Y)) * 0.5f;
//                     value.Y -= halfDiff;
//                     value.W += halfDiff;
//                 }
//                 base.Bounds = value;
//                 if (_isInitialized)
//                 {
//                     PositionElements();
//                     var view = ViewPort;
//                     foreach (var child in Children)
//                     {
//                         child.ClipBounds = view;
//                     }
//                 }
//             }
//         }

//         private bool _isInitialized = false;

//         private bool _refreshPending = false;

//         public bool multiSelect = false;

//         public override bool IsVisible
//         {
//             get => base.IsVisible;
//             set
//             {
//                 base.IsVisible = value;
//                 RefreshCurrentFolder();
//                 foreach (var child in Children)
//                 {
//                     child.IsVisible = value;
//                 }
//             }
//         }

//         public FileNavigator(UICanvas canvas, Vector4 bounds, float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(canvas, bounds, inset, uvInset, colour)
//         {
//             var view = ViewPort;
//             Search = new TextField(canvas, new Vector4(view.Z - textFieldWidth, view.W - componentHeight, view.Z, view.W), inset, uvInset);
//             Search.TextSize = componentHeight * 0.4f;
//             Search.Mode = TextFieldMode.SingleLine;
//             Search.OnTextChanged += RefreshCurrentFolder;
//             Confirm = new Button(canvas, new Vector4(view.Z - buttonWidth, view.Y, view.Z, view.Y + componentHeight), "Confirm", inset, uvInset);
//             Cancel = new Button(canvas, new Vector4(Confirm.Bounds.X - padding - buttonWidth, view.Y, Confirm.Bounds.X - padding, view.Y + componentHeight), "Cancel", inset, uvInset);
//             Back = new Button(canvas, new Vector4(view.X, view.W - componentHeight, view.X + buttonWidth, view.W), "Back", inset, uvInset);
//             Back.OnClick += _ =>
//             {
//                 NavigateUp();
//                 _refreshPending = true;
//             };
//             NameInput = new TextField(canvas, new Vector4(view.X, view.Y, Cancel.Bounds.X - padding, view.Y + componentHeight), inset, uvInset);
//             NameInput.TextSize = componentHeight * 0.4f;
//             NameInput.Mode = TextFieldMode.SingleLine;
//             QuickAccess = new Panel(canvas, new Vector4(view.X, NameInput.Bounds.W + padding, NameInput.Bounds.Z, Back.Bounds.Y - padding), new VerticalLayout(new Vector2(100, 35), 0), inset, uvInset);
//             CurrentDirectory = new Panel(canvas, new Vector4(QuickAccess.Bounds.Z + padding, QuickAccess.Bounds.Y, view.Z, QuickAccess.Bounds.W), new VerticalLayout(new Vector2(100, 35), 0), inset, uvInset);
//             AddChild(Search);
//             AddChild(NameInput);
//             AddChild(Confirm);
//             AddChild(Cancel);
//             AddChild(Back);
//             AddChild(QuickAccess);
//             AddChild(CurrentDirectory);
//             PositionElements();
//             var desktop = FileReference.SetUpFileRef(canvas, FileReference.Directories.Desktop);
//             var documents = FileReference.SetUpFileRef(canvas, FileReference.Directories.Documents);
//             var downloads = FileReference.SetUpFileRef(canvas, FileReference.Directories.Downloads);
//             var music = FileReference.SetUpFileRef(canvas, FileReference.Directories.Music);
//             var pictures = FileReference.SetUpFileRef(canvas, FileReference.Directories.Pictures);
//             var videos = FileReference.SetUpFileRef(canvas, FileReference.Directories.Videos);
//             BindQuickAccess(desktop);
//             BindQuickAccess(documents);
//             BindQuickAccess(downloads);
//             BindQuickAccess(music);
//             BindQuickAccess(pictures);
//             BindQuickAccess(videos);
//             _isInitialized = true;
//             RefreshCurrentFolder();
//         }

//         private void BindQuickAccess(FileReference reference)
//         {
//             QuickAccess.AddChild(reference);
//             reference.OnClick += _ =>
//             {
//                 CurrentPath = reference.ReferencePath;
//                 reference.selected = true;
//                 _refreshPending = true;
//             };
//         }

//         public void NavigateUp()
//         {
//             if (string.IsNullOrEmpty(CurrentPath))
//                 return;

//             string trimmed = CurrentPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

//             int lastSeparator = trimmed.LastIndexOfAny(
//             [
//                 Path.DirectorySeparatorChar,
//             Path.AltDirectorySeparatorChar
//             ]);

//             if (lastSeparator <= 0)
//                 return;

//             CurrentPath = trimmed[..lastSeparator];
//         }

//         public void AddChild(UIComponent child)
//         {
//             Canvas.Deregister(child);
//             child.ClipBounds = ViewPort;
//             Children.Add(child);
//         }

//         public void RemoveChild(UIComponent child)
//         {
//             Children.Remove(child);
//             child.ClipBounds = null;
//             Canvas.Register(child);
//         }

//         private void PositionElements()
//         {
//             var view = ViewPort;
//             // var rightPanelLeft = view.X + 
//             Back.Bounds = new Vector4(view.X, view.W - componentHeight, view.X + buttonWidth, view.W);
//             NameInput.Bounds = new Vector4(view.X, view.Y, view.X + textFieldWidth, view.Y + componentHeight);
//             Search.TextSize = componentHeight * 0.4f;
//             Search.Mode = TextFieldMode.SingleLine;
//             QuickAccess.Bounds = new Vector4(view.X, NameInput.Bounds.W + padding, NameInput.Bounds.Z, Back.Bounds.Y - padding);
//             Confirm.Bounds = new Vector4(view.Z - buttonWidth, view.Y, view.Z, view.Y + componentHeight);
//             Cancel.Bounds = new Vector4(Confirm.Bounds.X - padding - buttonWidth, view.Y, Confirm.Bounds.X - padding, view.Y + componentHeight);
//             Search.Bounds = new Vector4(view.Z - textFieldWidth, view.W - componentHeight, view.Z, view.W);
//             NameInput.TextSize = componentHeight * 0.4f;
//             NameInput.Mode = TextFieldMode.SingleLine;
//             CurrentDirectory.Bounds = new Vector4(QuickAccess.Bounds.Z + padding, view.Y + componentHeight + padding, view.Z, view.W - componentHeight - padding);
//         }

//         private void RefreshCurrentFolder()
//         {
//             if (!Search.IsFocused) Search.Text = "";
//             var filter = Search.Text.Trim();
//             CurrentDirectory.Clear();
//             CurrentDirectory.ScrollOffset = Vector2.Zero;
//             try
//             {
//                 var files = Directory.GetFiles(CurrentPath);
//                 var directories = Directory.GetDirectories(CurrentPath);
//                 if (directories is not null)
//                 {
//                     Array.Sort(directories);
//                     foreach (var directory in directories)
//                     {
//                         if (!string.IsNullOrEmpty(filter) && !Path.GetFileName(directory).Contains(filter, StringComparison.OrdinalIgnoreCase))
//                             continue;
//                         var directoryRef = FileReference.SetUpFileRef(Canvas, directory);
//                         directoryRef.IsVisible = IsVisible;
//                         directoryRef.Colour = directoryColour;
//                         CurrentDirectory.AddChild(directoryRef);
//                         directoryRef.DoubleClick += MouseButton =>
//                         {
//                             CurrentPath = directoryRef.ReferencePath;
//                             _refreshPending = true;
//                         };
//                         directoryRef.OnClick += MouseButton =>
//                         {
//                             if (!multiSelect) ClearSelected();
//                             directoryRef.selected = true;
//                         };
//                     }
//                 }
//                 if (files is not null)
//                 {
//                     Array.Sort(files);
//                     foreach (var file in files)
//                     {
//                         if (!string.IsNullOrEmpty(filter) && !Path.GetFileName(file).Contains(filter, StringComparison.OrdinalIgnoreCase))
//                             continue;
//                         var fileRef = FileReference.SetUpFileRef(Canvas, file);
//                         fileRef.IsVisible = IsVisible;
//                         fileRef.Colour = fileColour;
//                         CurrentDirectory.AddChild(fileRef);

//                         fileRef.OnClick += MouseButton =>
//                         {
//                             if (!multiSelect) ClearSelected();
//                             var selectedCount = SelectedFileCount;
//                             fileRef.selected = true;
//                             if (selectedCount >= 1) NameInput.Text = $"{selectedCount + 1} Files Selected.";
//                             else NameInput.Text = fileRef.Text;
//                         };
//                     }
//                 }
//                 SetQuickAccessSelection();
//             }
//             catch (UnauthorizedAccessException e)
//             {
//                 Console.WriteLine($"User does not have access to this file location: {e.Message}");
//             }
//         }

//         private void ClearSelected()
//         {
//             foreach (var element in CurrentDirectory.Children)
//             {
//                 if (element is FileReference fr)
//                 {
//                     fr.selected = false;
//                 }
//             }
//         }

//         private void SetQuickAccessSelection()
//         {
//             foreach (var element in QuickAccess.Children)
//             {
//                 if (element is FileReference fr)
//                 {
//                     fr.selected = fr.ReferencePath.Trim(Delimiter) == CurrentPath.Trim(Delimiter);
//                 }
//             }
//         }

//         private string[] GetSelectedPaths()
//         {
//             var list = new List<string>();
//             var selectedCount = SelectedFileCount;
//             if (selectedCount == 0 && NameInput.Text.Length > 0)
//             {
//                 list.Add($"{CurrentPath}{Delimiter}{NameInput.Text}");
//                 return [.. list];
//             }
//             foreach (var element in CurrentDirectory.Children)
//             {
//                 if (element is FileReference fr && fr.selected)
//                 {
//                     list.Add(fr.ReferencePath);
//                 }
//             }
//             return [.. list];
//         }

//         private Task<string[]?> GetPathFromPicker()
//         {
//             var tcs = new TaskCompletionSource<string[]?>();

//             IsVisible = true;

//             void CancelHandler(MouseButton btn)
//             {
//                 tcs.TrySetResult(null);
//                 Cleanup();
//                 IsVisible = false;
//             }

//             void ConfirmHandler(MouseButton btn)
//             {
//                 tcs.TrySetResult(GetSelectedPaths());
//                 Cleanup();
//                 IsVisible = false;
//             }

//             void Cleanup()
//             {
//                 Cancel.OnClick -= CancelHandler;
//                 Confirm.OnClick -= ConfirmHandler;
//             }

//             Cancel.OnClick += CancelHandler;
//             Confirm.OnClick += ConfirmHandler;

//             return tcs.Task;
//         }

//         public async Task<string[]?> SelectFile()
//         {
//             return await GetPathFromPicker();
//         }

//         public override bool OnClickDown(MouseState mouse)
//         {
//             if (!IsVisible || !WithinBounds(mouse))
//             {
//                 UICanvas.FocusedComponent = null;
//                 ClearSelected();
//                 return false;
//             }
//             foreach (var element in Children)
//             {
//                 bool result = element.OnClickDown(mouse);
//                 if (result) return true;
//             }

//             return base.OnClickDown(mouse);
//         }

//         public override bool OnClickUp(MouseState mouse)
//         {
//             if (!IsVisible || !WithinBounds(mouse)) return false;
//             foreach (var element in Children)
//             {
//                 bool result = element.OnClickUp(mouse);
//                 if (result) return true;
//             }
//             return base.OnClickUp(mouse);
//         }

//         public override bool OnMouseMove(MouseState mouse)
//         {
//             if (!IsVisible || !WithinBounds(mouse)) return false;
//             foreach (var element in Children)
//             {
//                 bool result = element.OnMouseMove(mouse);
//                 if (result) return true;
//             }
//             return base.OnMouseMove(mouse);
//         }

//         public override bool OnMouseWheel(MouseState mouse)
//         {
//             if (!IsVisible || !WithinBounds(mouse)) return false;
//             foreach (var element in Children)
//             {
//                 bool result = element.OnMouseWheel(mouse);
//                 if (result) return true;
//             }
//             return true;
//         }

//         public override void OnKeyDown(KeyboardKeyEventArgs e)
//         {
//             if (!IsVisible) return;
//             multiSelect = !Search.IsFocused && !NameInput.IsFocused && OperatingSystem.IsMacOS() ? e.Command : e.Control;
//             foreach (var element in Children)
//             {
//                 element.OnKeyDown(e);
//             }
//         }

//         public override void OnKeyUp(KeyboardKeyEventArgs e)
//         {
//             if (!IsVisible) return;
//             multiSelect = false;
//             foreach (var element in Children)
//             {
//                 element.OnKeyUp(e);
//             }
//         }

//         public override void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
//         {
//             if (!IsVisible) return;
//             foreach (var element in Children)
//             {
//                 element.OnUpdate(deltaTime, mouse, keyboard);
//             }
//             if (_refreshPending)
//             {
//                 RefreshCurrentFolder();
//                 _refreshPending = false;
//             }
//         }

//         public override void SubmitData(Renderer renderer)
//         {
//             if (!IsVisible) return;
//             base.SubmitData(renderer);
//             foreach (var element in Children)
//             {
//                 if (element is IRenderable renderable)
//                 {
//                     renderable.SubmitData(renderer);
//                 }
//             }
//         }
//     }

//     internal class FileReference : Button
//     {
//         public enum Directories
//         {
//             Recent,
//             Desktop,
//             Documents,
//             Downloads,
//             Music,
//             Pictures,
//             Videos,
//         }

//         public string ReferencePath
//         {
//             get;
//             set;
//         } = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

//         private static readonly double DoubleClickThreshold = 300; // milliseconds

//         private readonly Stopwatch clickTimer = new Stopwatch();

//         private bool clickPending = false;

//         public event Action<MouseButton>? DoubleClick;

//         public bool selected = false;

//         public Vector4 SelectionColor = new Vector4(0.2f, 0.6f, 1.0f, 1);

//         public override Vector4 Bounds
//         {
//             get => base.Bounds;
//             set
//             {
//                 base.Bounds = value;
//                 if (label is not null)
//                 {
//                     label.Origin = new Vector2(Bounds.X + Inset, label.Origin.Y);
//                     label.ClipBounds = value;
//                 }
//             }
//         }

//         public FileReference(UICanvas canvas, Vector4 bounds, string text = "", float inset = 10, float uvInset = 0.25F, Vector4? colour = null) : base(canvas, bounds, text, inset, uvInset, colour)
//         {
//             this.Canvas = canvas;
//             label.Alignment = TextAlignment.Left;
//             label.Origin = new Vector2(Bounds.X + Inset, label.Origin.Y);
//             label.ClipBounds = bounds;
//         }

//         public override bool OnClickDown(MouseState mouse)
//         {
//             if (!IsVisible) return false;
//             if (WithinBounds(mouse))
//             {
//                 if (clickPending && clickTimer.ElapsedMilliseconds <= DoubleClickThreshold)
//                 {
//                     clickPending = false;
//                     clickTimer.Reset();
//                     DoubleClick?.Invoke(MouseButton.Left);
//                 }
//                 else
//                 {
//                     clickPending = true;
//                     clickTimer.Restart();
//                 }
//                 isPressed = true;
//                 return true;
//             }
//             return false;
//         }

//         public override void OnUpdate(float deltaTime, MouseState mouse, KeyboardState keyboard)
//         {
//             base.OnUpdate(deltaTime, mouse, keyboard);
//             if (selected) ForceUpdateQuadrantColours(SelectionColor);
//             if (clickPending == true && clickTimer.ElapsedMilliseconds > DoubleClickThreshold)
//             {
//                 clickTimer.Reset();
//                 clickPending = false;
//             }
//         }

//         public bool IsDirectory
//         {
//             get => Directory.Exists(ReferencePath);
//         }

//         public bool IsFile
//         {
//             get => File.Exists(ReferencePath);
//         }

//         public static FileReference SetUpFileRef(UICanvas canvas, Directories directory)
//         {
//             FileReference temp = new FileReference(canvas, new Vector4());
//             temp.SetPath(directory);
//             temp.Text = GetNameOfPath(temp.ReferencePath);
//             temp.TimeToRollover = 0;
//             return temp;
//         }

//         public static FileReference SetUpFileRef(UICanvas canvas, string path)
//         {
//             FileReference temp = new FileReference(canvas, new Vector4());
//             temp.ReferencePath = path;
//             temp.Text = GetNameOfPath(temp.ReferencePath);
//             temp.TimeToRollover = 0.0f;
//             return temp;
//         }

//         private static string GetNameOfPath(string path)
//         {
//             if (Directory.Exists(path))
//             {
//                 var trimmed = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
//                 return Path.GetFileName(trimmed);
//             }
//             else if (File.Exists(path))
//             {
//                 return Path.GetFileName(path);
//             }
//             else
//             {
//                 Console.WriteLine($"not a directory or file: {path}");
//                 return "";
//             }
//         }

//         public void SetPath(Directories directory)
//         {
//             switch (directory)
//             {
//                 case Directories.Recent:
//                     ReferencePath = Environment.GetFolderPath(Environment.SpecialFolder.Recent);
//                     break;
//                 case Directories.Desktop:
//                     ReferencePath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
//                     break;
//                 case Directories.Documents:
//                     ReferencePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
//                     break;
//                 case Directories.Downloads:
//                     ReferencePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
//                     break;
//                 case Directories.Music:
//                     ReferencePath = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
//                     break;
//                 case Directories.Pictures:
//                     ReferencePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
//                     break;
//                 case Directories.Videos:
//                     ReferencePath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
//                     break;
//             }
//         }
//     }
// }