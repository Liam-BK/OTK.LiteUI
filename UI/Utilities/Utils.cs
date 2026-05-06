using OpenTK.Mathematics;

public static class Utils
{
    public static UIQuad Clip(UIQuad quad, Vector4? clipBounds)
    {
        if (clipBounds is not Vector4 clip) return quad;
        float halfWidth = quad.size.X * 0.5f;
        float halfHeight = quad.size.Y * 0.5f;
        float left = quad.position.X - halfWidth;
        float bottom = quad.position.Y - halfHeight;
        float right = quad.position.X + halfWidth;
        float top = quad.position.Y + halfHeight;
        float width = right - left;
        float height = top - bottom;

        if (width <= 0 || height <= 0)
        {
            return quad;
        }

        float nLeft = MathF.Max(left, clip.X);
        float nRight = MathF.Min(right, clip.Z);
        float nBottom = MathF.Max(bottom, clip.Y);
        float nTop = MathF.Min(top, clip.W);

        float uMin = (nLeft - left) / width;
        float uMax = (nRight - left) / width;
        float vMin = (nBottom - bottom) / height;
        float vMax = (nTop - bottom) / height;

        if (nLeft >= nRight || nBottom >= nTop)
            return default;

        Vector2 newSize = new Vector2(nRight - nLeft, nTop - nBottom);

        Vector2 newPos = new Vector2(
            nLeft + newSize.X * 0.5f,
            nBottom + newSize.Y * 0.5f
        );

        Vector2 newUVOffset = new Vector2(
            quad.UVOffset.X + quad.UVRange.X * uMin,
            quad.UVOffset.Y + quad.UVRange.Y * vMin
        );

        Vector2 newUVRange = new Vector2(
            quad.UVRange.X * (uMax - uMin),
            quad.UVRange.Y * (vMax - vMin)
        );

        quad.position = newPos;
        quad.size = newSize;
        quad.UVOffset = newUVOffset;
        quad.UVRange = newUVRange;

        return quad;
    }

    public static bool ClipOverlapsQuad(Vector4 clip, UIQuad quad)
    {
        float left = quad.position.X - quad.size.X * 0.5f;
        float bottom = quad.position.Y - quad.size.Y * 0.5f;
        float right = left + quad.size.X;
        float top = bottom + quad.size.Y;
        return !(left > clip.Z || bottom > clip.W || right < clip.X || top < clip.Y);
    }
}