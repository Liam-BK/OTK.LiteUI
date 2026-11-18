using OpenTK.Graphics.ES11;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;

namespace OTK.LiteUI.UI.Utilities
{
    internal static class AutoRegistry
    {
        private static readonly Dictionary<string, Type> Components = new();
        private static readonly Dictionary<string, Type> Layouts = new();

        private static bool _isInitialized = false;

        public static void Initialize()
        {
            if (_isInitialized) return;
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(assembly => assembly.GetTypes()).Where(t => t.IsClass && !t.IsAbstract && (typeof(UIElement).IsAssignableFrom(t) || typeof(ILayout).IsAssignableFrom(t)));
            foreach (var type in types)
            {
                if (typeof(UIElement).IsAssignableFrom(type))
                {
                    Components[type.Name] = type;
                }
                else if (typeof(ILayout).IsAssignableFrom(type))
                {
                    Layouts[type.Name] = type;
                }
            }
            Console.WriteLine($"component count: {Components.Count}");
            Console.WriteLine($"layout count: {Layouts.Count}");
            _isInitialized = true;
        }

        public static void RegisterComponent<T>() where T : UIElement
        {
            Components[typeof(T).Name] = typeof(T);
        }

        public static void RegisterLayout<T>() where T : ILayout
        {
            Layouts[typeof(T).Name] = typeof(T);
        }

        public static bool TryGetComponent(string name, out Type? type)
        {
            return Components.TryGetValue(name, out type);
        }

        public static bool TryGetLayout(string name, out Type? type)
        {
            return Layouts.TryGetValue(name, out type);
        }
    }
}