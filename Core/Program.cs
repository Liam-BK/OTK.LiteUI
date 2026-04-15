using OpenTK.Windowing.Desktop;

public class Program
{
    static void Main()
    {
        Directory.SetCurrentDirectory(Path.Combine(AppContext.BaseDirectory, @"../../../"));
        using (var mainPanel = new MainPanel(new GameWindowSettings(), new NativeWindowSettings() { NumberOfSamples = 4 }))
        {
            mainPanel.Run();
        }
    }
}
