using OpenTK.Mathematics;

namespace OTK.LiteUI.UI.Rendering
{
    /// <summary>
    /// Stores the per-instance rendering data for a UI quad, including its
    /// position, dimensions, texture coordinates, colour, and texture array layer.
    /// </summary>
    public struct UIQuad : IInstanceData
    {
        /// <summary>
        /// The position of the center of the quad.
        /// </summary>
        public Vector2 position;

        /// <summary>
        /// The dimensions of the quad.
        /// </summary>
        public Vector2 size;

        /// <summary>
        /// The starting texture coordinates within the texture.
        /// </summary>
        public Vector2 UVOffset;

        /// <summary>
        /// The texture coordinate range of the quad.
        /// </summary>
        public Vector2 UVRange;

        /// <summary>
        /// The colour applied to the quad.
        /// </summary>
        public Vector4 colour;

        /// <summary>
        /// The texture array layer used by the quad.
        /// </summary>
        public float textureLayer;

        /// <summary>
        /// Writes the quad's instance data to the specified buffer in rendering order.
        /// </summary>
        /// <param name="ToPack">The buffer receiving the packed instance data.</param>
        public void Pack(List<float> ToPack)
        {
            Instancing.WriteToBuffer(ToPack, position);
            Instancing.WriteToBuffer(ToPack, size);
            Instancing.WriteToBuffer(ToPack, UVOffset);
            Instancing.WriteToBuffer(ToPack, UVRange);
            Instancing.WriteToBuffer(ToPack, colour);
            Instancing.WriteToBuffer(ToPack, textureLayer);
        }
    }
}