namespace FluxJpeg.Core
{
    using System;

    internal class ZigZag
    {
        internal static readonly int[] ZigZagMap = new int[] { 
            0, 1, 8, 0x10, 9, 2, 3, 10, 0x11, 0x18, 0x20, 0x19, 0x12, 11, 4, 5, 
            12, 0x13, 0x1a, 0x21, 40, 0x30, 0x29, 0x22, 0x1b, 20, 13, 6, 7, 14, 0x15, 0x1c, 
            0x23, 0x2a, 0x31, 0x38, 0x39, 50, 0x2b, 0x24, 0x1d, 0x16, 15, 0x17, 30, 0x25, 0x2c, 0x33, 
            0x3a, 0x3b, 0x34, 0x2d, 0x26, 0x1f, 0x27, 0x2e, 0x35, 60, 0x3d, 0x36, 0x2f, 0x37, 0x3e, 0x3f
         };

        public static void UnZigZag(float[] input, float[] output)
        {
            output[0] = input[0];
            output[1] = input[1];
            output[8] = input[2];
            output[0x10] = input[3];
            output[9] = input[4];
            output[2] = input[5];
            output[3] = input[6];
            output[10] = input[7];
            output[0x11] = input[8];
            output[0x18] = input[9];
            output[0x20] = input[10];
            output[0x19] = input[11];
            output[0x12] = input[12];
            output[11] = input[13];
            output[4] = input[14];
            output[5] = input[15];
            output[12] = input[0x10];
            output[0x13] = input[0x11];
            output[0x1a] = input[0x12];
            output[0x21] = input[0x13];
            output[40] = input[20];
            output[0x30] = input[0x15];
            output[0x29] = input[0x16];
            output[0x22] = input[0x17];
            output[0x1b] = input[0x18];
            output[20] = input[0x19];
            output[13] = input[0x1a];
            output[6] = input[0x1b];
            output[7] = input[0x1c];
            output[14] = input[0x1d];
            output[0x15] = input[30];
            output[0x1c] = input[0x1f];
            output[0x23] = input[0x20];
            output[0x2a] = input[0x21];
            output[0x31] = input[0x22];
            output[0x38] = input[0x23];
            output[0x39] = input[0x24];
            output[50] = input[0x25];
            output[0x2b] = input[0x26];
            output[0x24] = input[0x27];
            output[0x1d] = input[40];
            output[0x16] = input[0x29];
            output[15] = input[0x2a];
            output[0x17] = input[0x2b];
            output[30] = input[0x2c];
            output[0x25] = input[0x2d];
            output[0x2c] = input[0x2e];
            output[0x33] = input[0x2f];
            output[0x3a] = input[0x30];
            output[0x3b] = input[0x31];
            output[0x34] = input[50];
            output[0x2d] = input[0x33];
            output[0x26] = input[0x34];
            output[0x1f] = input[0x35];
            output[0x27] = input[0x36];
            output[0x2e] = input[0x37];
            output[0x35] = input[0x38];
            output[60] = input[0x39];
            output[0x3d] = input[0x3a];
            output[0x36] = input[0x3b];
            output[0x2f] = input[60];
            output[0x37] = input[0x3d];
            output[0x3e] = input[0x3e];
            output[0x3f] = input[0x3f];
        }
    }
}

