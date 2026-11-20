namespace OTK.LiteUI.Core
{
    /// <summary>
    /// Sets a property as eligible for assignment through the JSON UI loader. Intended for properties containing basic values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public sealed class UIPropertyAttribute : Attribute
    {

    }
}