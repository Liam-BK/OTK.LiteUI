using OpenTK.Mathematics;

namespace OTK.LiteUI.UI.Rendering
{
    /// <summary>
    /// Defines data that can be packed into a buffer for instanced rendering.
    /// </summary>
    public interface IInstanceData
    {
        /// <summary>
        /// Writes the instance data to the specified buffer.
        /// </summary>
        /// <param name="ToPack">The buffer receiving the packed data.</param>
        public void Pack(List<float> ToPack);
    }

    /// <summary>
    /// Provides helper methods for writing primitive and OpenTK mathematical types
    /// to a floating-point buffer for instanced rendering.
    /// </summary>
    public static class Instancing
    {
        /// <summary>
        /// Appends the specified value to the buffer.
        /// </summary>
        /// <param name="ToPack">The buffer receiving the value.</param>
        /// <param name="value">The value to append.</param>
        public static void WriteToBuffer(List<float> ToPack, float value)
        {
            ToPack.Add(value);
        }

        /// <summary>
        /// Appends the specified integer value to the buffer.
        /// </summary>
        /// <param name="ToPack">The buffer receiving the value, converted to a float.</param>
        /// <param name="value">The integer value to append.</param>
        public static void WriteToBuffer(List<float> ToPack, int value)
        {
            ToPack.Add(value);
        }

        /// <summary>
        /// Appends the vector components to the buffer in X, Y order.
        /// </summary>
        /// <param name="ToPack">The buffer receiving the components.</param>
        /// <param name="value">The vector to append.</param>
        public static void WriteToBuffer(List<float> ToPack, Vector2 value)
        {
            ToPack.Add(value.X);
            ToPack.Add(value.Y);
        }

        /// <summary>
        /// Appends the vector components to the buffer in X, Y, Z order.
        /// </summary>
        /// <param name="ToPack">The buffer receiving the components.</param>
        /// <param name="value">The vector to append.</param>
        public static void WriteToBuffer(List<float> ToPack, Vector3 value)
        {
            ToPack.Add(value.X);
            ToPack.Add(value.Y);
            ToPack.Add(value.Z);
        }

        /// <summary>
        /// Appends the vector components to the buffer in X, Y, Z, W order.
        /// </summary>
        /// <param name="ToPack">The buffer receiving the components.</param>
        /// <param name="value">The vector to append.</param>
        public static void WriteToBuffer(List<float> ToPack, Vector4 value)
        {
            ToPack.Add(value.X);
            ToPack.Add(value.Y);
            ToPack.Add(value.Z);
            ToPack.Add(value.W);
        }

        /// <summary>
        /// Appends the matrix columns to the buffer in column-major order.
        /// </summary>
        /// <param name="ToPack">The buffer receiving the components.</param>
        /// <param name="value">The matrix to append.</param>
        public static void WriteToBuffer(List<float> ToPack, Matrix4 value)
        {
            WriteToBuffer(ToPack, value.Column0);
            WriteToBuffer(ToPack, value.Column1);
            WriteToBuffer(ToPack, value.Column2);
            WriteToBuffer(ToPack, value.Column3);
        }
    }
}