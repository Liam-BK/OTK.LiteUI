using StbImageSharp;

public enum EmptyPixelType
{
    Black,
    White,
    Transparent
}

public class ImageData
{
    /// <summary>
    /// The width of a bitmap in pixels.
    /// </summary>
    public int Width;

    /// <summary>
    /// The height of a bitmap in pixels.
    /// </summary>
    public int Height;

    /// <summary>
    /// An array of bytes representing the pixel data of a bitmap.
    /// </summary>
    public byte[] Pixels = Array.Empty<byte>();

    /// <summary>
    /// The number of color channels for the bitmap.
    /// </summary>
    public int Channels;

    /// <summary>
    /// A method for flipping the bits of an image vertically. 
    /// Useful for converting between coordinate systems.
    /// </summary>
    public void FlipImageVertically()
    {
        int width = Width;
        int height = Height;
        int channels = Channels;
        byte[] pixels = Pixels;

        int stride = width * channels;
        byte[] rowBuffer = new byte[stride];

        for (int y = 0; y < height / 2; y++)
        {
            int top = y * stride;
            int bottom = (height - 1 - y) * stride;

            // Swap rows
            Array.Copy(pixels, top, rowBuffer, 0, stride);
            Array.Copy(pixels, bottom, pixels, top, stride);
            Array.Copy(rowBuffer, 0, pixels, bottom, stride);
        }
    }

    /// <summary>
    /// Converts a given bitmap to a greyscale representation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the image has less than 3 color channels.</exception>
    public void GreyScale()
    {
        if (Channels < 3)
            throw new InvalidOperationException("Image must have at least 3 channels for greyscale conversion.");

        for (int i = 0; i < Pixels.Length; i += Channels)
        {
            byte r = Pixels[i];
            byte g = Pixels[i + 1];
            byte b = Pixels[i + 2];

            // Calculate luminance using perceptual weighting
            byte grey = (byte)(0.3 * r + 0.59 * g + 0.11 * b);

            Pixels[i] = grey;
            Pixels[i + 1] = grey;
            Pixels[i + 2] = grey;
            // Leave alpha unchanged (i + 3)
        }
    }

    public void ConvertToResolution(TextureResolution resolution, EmptyPixelType emptyType)
    {
        int target = TextureManager.FindResolution(resolution);

        if (Width == target && Height == target)
            return;

        // Step 1: if larger → downscale
        if (Width > target || Height > target)
        {
            Downscale(target, target);
        }

        // Step 2: if smaller → pad
        if (Width < target || Height < target)
        {
            PadTo(target, target, emptyType);
        }
    }

    private void PadTo(int targetWidth, int targetHeight, EmptyPixelType emptyType)
    {
        byte[] newPixels = new byte[targetWidth * targetHeight * Channels];

        var fill = GetFillPixel(emptyType);

        // fill entire buffer
        for (int i = 0; i < newPixels.Length; i += Channels)
        {
            for (int c = 0; c < Channels; c++)
                newPixels[i + c] = fill[c];
        }

        int offsetX = (targetWidth - Width) / 2;
        int offsetY = (targetHeight - Height) / 2;

        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                int srcIndex = (y * Width + x) * Channels;
                int dstIndex = ((y + offsetY) * targetWidth + (x + offsetX)) * Channels;

                for (int c = 0; c < Channels; c++)
                {
                    newPixels[dstIndex + c] = Pixels[srcIndex + c];
                }
            }
        }

        Pixels = newPixels;
        Width = targetWidth;
        Height = targetHeight;
    }

    private void Downscale(int targetWidth, int targetHeight)
    {
        byte[] newPixels = new byte[targetWidth * targetHeight * Channels];

        float scaleX = (float)Width / targetWidth;
        float scaleY = (float)Height / targetHeight;

        for (int y = 0; y < targetHeight; y++)
        {
            for (int x = 0; x < targetWidth; x++)
            {
                int srcX = (int)(x * scaleX);
                int srcY = (int)(y * scaleY);

                int srcIndex = (srcY * Width + srcX) * Channels;
                int dstIndex = (y * targetWidth + x) * Channels;

                for (int c = 0; c < Channels; c++)
                {
                    newPixels[dstIndex + c] = Pixels[srcIndex + c];
                }
            }
        }

        Pixels = newPixels;
        Width = targetWidth;
        Height = targetHeight;
    }

    private byte[] GetFillPixel(EmptyPixelType type)
    {
        return type switch
        {
            EmptyPixelType.Black => [0, 0, 0, 255],
            EmptyPixelType.White => [255, 255, 255, 255],
            EmptyPixelType.Transparent => [0, 0, 0, 0],
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public static class ImageLoader
{
    [Flags]
    public enum Flip
    {
        None = 0,
        Vertical = 1 << 1,
        Horizontal = 1 << 2
    }


    private static bool IsFlipVertical(Flip flipFlag)
    {
        return (flipFlag & Flip.Vertical) != 0;
    }

    /// <summary>
    /// Is currently unused
    /// </summary>
    /// <param name="flipFlag"></param>
    /// <returns></returns>
    public static bool IsFlipHorizontal(Flip flipFlag)
    {
        return (flipFlag & Flip.Horizontal) != 0;
    }

    /// <summary>
    /// Loads a png file and returns an ImageData representing the loaded file.
    /// </summary>
    /// <param name="filePath">The path to the image.</param>
    /// <param name="flipFlag">Determines whether or not to flip the image vertically.</param>
    /// <param name="greyscale">Sets the image to greyscale if true.</param>
    /// <returns>An instance of ImageData.</returns>
    public static ImageData LoadImage(string filePath, Flip flipFlag, bool greyscale = false)
    {
        using Stream? stream = ResourceIO.GetLoadingStream(filePath);
        if (stream == null)
            throw new FileNotFoundException($"Image file not found: {filePath}");

        var image = ImageResult.FromStream(stream, ColorComponents.RedGreenBlueAlpha);
        var temp = new ImageData
        {
            Width = image.Width,
            Height = image.Height,
            Channels = 4,
            Pixels = image.Data
        };
        if (IsFlipVertical(flipFlag)) temp.FlipImageVertically();
        if (greyscale) temp.GreyScale();
        return temp;
    }
}