namespace FluxJpeg.Core
{
    using System;

    internal class JpegQuantizationTable
    {
        public static JpegQuantizationTable K1Div2Luminance = K1Luminance.getScaledInstance(0.5f, true);
        public static JpegQuantizationTable K1Luminance = new JpegQuantizationTable(new int[] { 
            0x10, 11, 10, 0x10, 0x18, 40, 0x33, 0x3d, 12, 12, 14, 0x13, 0x1a, 0x3a, 60, 0x37, 
            14, 13, 0x10, 0x18, 40, 0x39, 0x45, 0x38, 14, 0x11, 0x16, 0x1d, 0x33, 0x57, 80, 0x3e, 
            0x12, 0x16, 0x25, 0x38, 0x44, 0x6d, 0x67, 0x4d, 0x18, 0x23, 0x37, 0x40, 0x51, 0x68, 0x71, 0x5c, 
            0x31, 0x40, 0x4e, 0x57, 0x67, 0x79, 120, 0x65, 0x48, 0x5c, 0x5f, 0x62, 0x70, 100, 0x67, 0x63
         }, false);
        public static JpegQuantizationTable K2Chrominance = new JpegQuantizationTable(new int[] { 
            0x11, 0x12, 0x18, 0x2f, 0x63, 0x63, 0x63, 0x63, 0x12, 0x15, 0x1a, 0x42, 0x63, 0x63, 0x63, 0x63, 
            0x18, 0x1a, 0x38, 0x63, 0x63, 0x63, 0x63, 0x63, 0x2f, 0x42, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 
            0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 
            0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63, 0x63
         }, false);
        public static JpegQuantizationTable K2Div2Chrominance = K2Chrominance.getScaledInstance(0.5f, true);
        private int[] table;

        public JpegQuantizationTable(int[] table) : this(checkTable(table), true)
        {
        }

        private JpegQuantizationTable(int[] table, bool copy)
        {
            this.table = copy ? ((int[]) table.Clone()) : table;
        }

        private static int[] checkTable(int[] table)
        {
            if ((table == null) || (table.Length != 0x40))
            {
                throw new ArgumentException("Invalid JPEG quantization table");
            }
            return table;
        }

        public JpegQuantizationTable getScaledInstance(float scaleFactor, bool forceBaseline)
        {
            int[] table = (int[]) this.table.Clone();
            int num = forceBaseline ? 0xff : 0x7fff;
            for (int i = 0; i < table.Length; i++)
            {
                table[i] = (int) Math.Round((double) (scaleFactor * table[i]));
                if (table[i] < 1)
                {
                    table[i] = 1;
                }
                else if (table[i] > num)
                {
                    table[i] = num;
                }
            }
            return new JpegQuantizationTable(table, false);
        }

        public int[] Table
        {
            get
            {
                return this.table;
            }
        }
    }
}

