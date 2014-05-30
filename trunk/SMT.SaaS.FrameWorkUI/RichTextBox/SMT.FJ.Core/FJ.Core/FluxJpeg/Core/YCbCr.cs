namespace FluxJpeg.Core
{
    using System;

    internal class YCbCr
    {
        public static float[] fromRGB(float[] data)
        {
            return new float[] { ((float) (((0.299 * data[0]) + (0.587 * data[1])) + (0.114 * data[2]))), (128f + ((float) (((-0.16874 * data[0]) - (0.33126 * data[1])) + (0.5 * data[2])))), (128f + ((float) (((0.5 * data[0]) - (0.41869 * data[1])) - (0.08131 * data[2])))) };
        }

        public static void fromRGB(ref byte c1, ref byte c2, ref byte c3)
        {
            double num = (double) c1;
            double num2 = (double) c2;
            double num3 = (double) c3;
            c1 = (byte) (((0.299 * num) + (0.587 * num2)) + (0.114 * num3));
            c2 = (byte) ((((-0.16874 * num) - (0.33126 * num2)) + (0.5 * num3)) + 128.0);
            c3 = (byte) ((((0.5 * num) - (0.41869 * num2)) - (0.08131 * num3)) + 128.0);
        }

        public static void toRGB(ref byte c1, ref byte c2, ref byte c3)
        {
            double num = (double) c1;
            double num2 = ((double) c2) - 128.0;
            double num3 = ((double) c3) - 128.0;
            double num4 = num + (1.402 * num3);
            double num5 = (num - (0.34414 * num2)) - (0.71414 * num3);
            double num6 = num + (1.772 * num2);
            c1 = (num4 > 255.0) ? ((byte) 0xff) : ((num4 < 0.0) ? ((byte) 0) : ((byte) num4));
            c2 = (num5 > 255.0) ? ((byte) 0xff) : ((num5 < 0.0) ? ((byte) 0) : ((byte) num5));
            c3 = (num6 > 255.0) ? ((byte) 0xff) : ((num6 < 0.0) ? ((byte) 0) : ((byte) num6));
        }
    }
}

