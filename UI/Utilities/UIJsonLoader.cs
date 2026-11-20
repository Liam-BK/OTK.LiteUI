using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using OpenTK.Mathematics;
using OTK.LiteUI.Core;
using OTK.LiteUI.UI.Components;
using OTK.LiteUI.UI.Interfaces;
using OTK.LiteUI.UI.Utilities;

internal class UIJsonLoader
{
    private readonly UICanvas _canvas;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public UIJsonLoader(UICanvas canvas)
    {
        _canvas = canvas;
    }

    public void Load(Stream inputStream)
    {
        using var document = JsonDocument.Parse(inputStream);
        var root = document.RootElement;

        if (root.ValueKind == JsonValueKind.Array)
        {
            foreach (var element in root.EnumerateArray())
                CreateComponent(element);
        }
        else if (root.ValueKind == JsonValueKind.Object)
        {
            CreateComponent(root);
        }
        else
        {
            throw new JsonException(
                "Expected a UI component object or an array of UI components.");
        }
    }

    private void ApplyProperties(object component, JsonElement properties)
    {
        var componentProperties = component.GetType().GetProperties();

        foreach (var property in componentProperties)
        {
            if (!Attribute.IsDefined(property, typeof(UIPropertyAttribute)))
                continue;

            if (!property.CanWrite)
                continue;

            if (!properties.TryGetProperty(property.Name, out var jsonValue))
                continue;

            try
            {
                object? value;
                if (typeof(ILayout).IsAssignableFrom(property.PropertyType)) value = CreateLayout(jsonValue);
                else value = ConvertJsonValue(jsonValue, property.PropertyType);
                property.SetValue(component, value);
            }
            catch (Exception ex)
            {
                throw new JsonException(
                    $"Failed to set property '{property.Name}' on component '{component.GetType().Name}'.",
                    ex);
            }
        }
    }

    private static object? ConvertJsonValue(JsonElement json, Type targetType)
    {
        var underlyingType = Nullable.GetUnderlyingType(targetType);
        if (underlyingType is not null)
        {
            if (json.ValueKind == JsonValueKind.Null)
                return null;

            targetType = underlyingType;
        }
        if (targetType == typeof(Vector2))
        {
            var f = CheckFloatArray(json, 2);
            return new Vector2(f[0], f[1]);
        }
        else if (targetType == typeof(Vector3))
        {
            var f = CheckFloatArray(json, 3);
            return new Vector3(f[0], f[1], f[2]);
        }
        else if (targetType == typeof(Vector4))
        {
            var f = CheckFloatArray(json, 4);
            return new Vector4(f[0], f[1], f[2], f[3]);
        }
        else if (targetType == typeof(Matrix2))
        {
            var f = CheckFloatArray(json, 4);
            return new Matrix2(
                f[0], f[1],
                f[2], f[3]
            );
        }
        else if (targetType == typeof(Matrix2x3))
        {
            var f = CheckFloatArray(json, 6);
            return new Matrix2x3(
                f[0], f[1],
                f[2], f[3],
                f[4], f[5]
            );
        }
        else if (targetType == typeof(Matrix2x4))
        {
            var f = CheckFloatArray(json, 8);
            return new Matrix2x4(
                f[0], f[1],
                f[2], f[3],
                f[4], f[5],
                f[6], f[7]
            );
        }
        else if (targetType == typeof(Matrix3x2))
        {
            var f = CheckFloatArray(json, 6);
            return new Matrix3x2(
                f[0], f[1], f[2],
                f[3], f[4], f[5]
            );
        }
        else if (targetType == typeof(Matrix3))
        {
            var f = CheckFloatArray(json, 9);
            return new Matrix3(
                f[0], f[1], f[2],
                f[3], f[4], f[5],
                f[6], f[7], f[8]
            );
        }
        else if (targetType == typeof(Matrix3x4))
        {
            var f = CheckFloatArray(json, 12);
            return new Matrix3x4(
                f[0], f[1], f[2],
                f[3], f[4], f[5],
                f[6], f[7], f[8],
                f[9], f[10], f[11]
            );
        }
        else if (targetType == typeof(Matrix4x2))
        {
            var f = CheckFloatArray(json, 8);
            return new Matrix4x2(
                f[0], f[1], f[2], f[3],
                f[4], f[5], f[6], f[7]
            );
        }
        else if (targetType == typeof(Matrix4x3))
        {
            var f = CheckFloatArray(json, 12);
            return new Matrix4x3(
                f[0], f[1], f[2], f[3],
                f[4], f[5], f[6], f[7],
                f[8], f[9], f[10], f[11]
            );
        }
        else if (targetType == typeof(Matrix4))
        {
            var f = CheckFloatArray(json, 16);
            return new Matrix4(
                f[0], f[1], f[2], f[3],
                f[4], f[5], f[6], f[7],
                f[8], f[9], f[10], f[11],
                f[12], f[13], f[14], f[15]
            );
        }

        return JsonSerializer.Deserialize(json.GetRawText(), targetType, JsonOptions);
    }

    private static float[] CheckFloatArray(JsonElement json, int expectedLength)
    {
        if (json.ValueKind != JsonValueKind.Array)
            throw new JsonException("Expected a JSON array.");

        if (json.GetArrayLength() != expectedLength)
            throw new JsonException(
                $"Expected {expectedLength} values, but received {json.GetArrayLength()}.");

        return json.EnumerateArray()
            .Select(x => x.GetSingle())
            .ToArray();
    }

    private object?[]? ResolveConstructorArguments(ConstructorInfo constructor, JsonElement json)
    {
        if (!TryGetProperty(json, "Properties", out var properties))
            return null;

        var parameters = constructor.GetParameters();
        var arguments = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var parameter = parameters[i];

            if (parameter.ParameterType == typeof(UICanvas))
            {
                arguments[i] = _canvas;
                continue;
            }

            if (TryGetProperty(properties, parameter.Name!, out var jsonValue))
            {
                if (parameter.ParameterType == typeof(ILayout)) arguments[i] = CreateLayout(jsonValue);
                else arguments[i] = ConvertJsonValue(jsonValue, parameter.ParameterType);

                continue;
            }

            if (parameter.HasDefaultValue)
            {
                arguments[i] = parameter.DefaultValue;
                continue;
            }

            return null;
        }

        return arguments;
    }

    private static bool TryGetProperty(JsonElement json, string name, out JsonElement value)
    {
        foreach (var property in json.EnumerateObject())
        {
            if (string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }

    private UIElement CreateComponent(JsonElement json)
    {
        string typeName = json.GetProperty("Type").GetString()
            ?? throw new JsonException("Missing component type.");

        if (!AutoRegistry.TryGetComponent(typeName, out var type))
            throw new JsonException($"Unknown component type '{typeName}'.");

        var matches = type!.GetConstructors().Select(c => (Constructor: c, Arguments: ResolveConstructorArguments(c, json), HasCanvas: c.GetParameters().Any(p => p.ParameterType == typeof(UICanvas)))).Where(x => x.Arguments is not null).OrderByDescending(x => x.HasCanvas).ToList();

        if (matches.Count == 0)
            throw new JsonException(
                $"No suitable constructor found for component '{typeName}'.");

        if (matches.Count > 1 && matches[0].HasCanvas == matches[1].HasCanvas)
            throw new JsonException(
                $"Multiple suitable constructors found for component '{typeName}'.");

        try
        {
            var bestMatch = matches[0];
            var component = (UIElement)bestMatch.Constructor.Invoke(bestMatch.Arguments);

            if (TryGetProperty(json, "Properties", out var properties))
                ApplyProperties(component, properties);

            if (component is IUIContainer container && TryGetProperty(properties, "Children", out var children))
            {
                if (children.ValueKind != JsonValueKind.Array)
                    throw new JsonException("'Children' must be an array.");
                foreach (var child in children.EnumerateArray())
                {
                    var childElement = CreateComponent(child);
                    if (childElement is UIComponent childComponent) container.AddChild(childComponent);
                }
            }
            return component;
        }
        catch (TargetInvocationException ex)
        {
            throw new JsonException(
                $"Failed to construct component '{typeName}'.",
                ex.InnerException ?? ex);
        }
    }

    private ILayout CreateLayout(JsonElement json)
    {
        string typeName = json.GetProperty("Type").GetString()
            ?? throw new JsonException("Missing layout type.");

        if (!AutoRegistry.TryGetLayout(typeName, out var type))
            throw new JsonException($"Unknown layout type '{typeName}'.");

        var matches = type!.GetConstructors().Select(c => (Constructor: c, Arguments: ResolveConstructorArguments(c, json))).Where(x => x.Arguments is not null).ToList();

        if (matches.Count == 0)
            throw new JsonException(
                $"No suitable constructor found for layout '{typeName}'.");

        if (matches.Count > 1)
            throw new JsonException(
                $"Multiple suitable constructors found for layout '{typeName}'.");

        try
        {
            var bestMatch = matches[0];
            var layout = (ILayout)bestMatch.Constructor.Invoke(bestMatch.Arguments);

            if (TryGetProperty(json, "Properties", out var properties))
                ApplyProperties(layout, properties);

            return layout;
        }
        catch (TargetInvocationException ex)
        {
            throw new JsonException(
                $"Failed to construct component '{typeName}'.",
                ex.InnerException ?? ex);
        }
    }
}