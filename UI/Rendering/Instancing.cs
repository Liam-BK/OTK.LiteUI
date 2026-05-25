using OpenTK.Mathematics;

namespace OTK.LiteUI.UI.Rendering
{
    public interface IInstanceData
    {
        public void Pack(List<float> ToPack);
    }

    public static class Instancing
    {
        public static void WriteToBuffer(List<float> ToPack, float value)
        {
            ToPack.Add(value);
        }

        public static void WriteToBuffer(List<float> ToPack, int value)
        {
            ToPack.Add(value);
        }

        public static void WriteToBuffer(List<float> ToPack, Vector2 value)
        {
            ToPack.Add(value.X);
            ToPack.Add(value.Y);
        }

        public static void WriteToBuffer(List<float> ToPack, Vector3 value)
        {
            ToPack.Add(value.X);
            ToPack.Add(value.Y);
            ToPack.Add(value.Z);
        }

        public static void WriteToBuffer(List<float> ToPack, Vector4 value)
        {
            ToPack.Add(value.X);
            ToPack.Add(value.Y);
            ToPack.Add(value.Z);
            ToPack.Add(value.W);
        }

        public static void WriteToBuffer(List<float> ToPack, Matrix4 value)
        {
            WriteToBuffer(ToPack, value.Column0);
            WriteToBuffer(ToPack, value.Column1);
            WriteToBuffer(ToPack, value.Column2);
            WriteToBuffer(ToPack, value.Column3);
        }
    }
}