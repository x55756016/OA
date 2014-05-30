namespace FluxJpeg.Core
{
    using FluxJpeg.Core.IO;
    using System;
    using System.Collections.Generic;
    using System.IO;

    internal class HuffmanTable
    {
        internal int[][,] AC_matrix;
        internal int[,] AC_matrix0;
        internal int[,] AC_matrix1;
        private short[] bits;
        public List<short[]> bitsList;
        private int bufferPutBits;
        private int bufferPutBuffer;
        internal int[][,] DC_matrix;
        internal int[,] DC_matrix0;
        internal int[,] DC_matrix1;
        private short[] huffcode = new short[0x100];
        public static int HUFFMAN_MAX_TABLES = 4;
        private short[] huffsize = new short[0x100];
        private short[] huffval;
        internal int ImageHeight;
        internal int ImageWidth;
        public static byte JPEG_AC_TABLE = 1;
        public static byte JPEG_DC_TABLE = 0;
        private short lastk = 0;
        private short[] maxcode = new short[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
        private short[] mincode = new short[] { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
        internal int NumOfACTables;
        internal int NumOfDCTables;
        public List<short[]> val;
        private short[] valptr = new short[0x10];

        internal HuffmanTable(JpegHuffmanTable table)
        {
            if (table != null)
            {
                this.huffval = table.Values;
                this.bits = table.Lengths;
                this.GenerateSizeTable();
                this.GenerateCodeTable();
                this.GenerateDecoderTables();
            }
            else
            {
                this.bitsList = new List<short[]>();
                this.bitsList.Add(JpegHuffmanTable.StdDCLuminance.Lengths);
                this.bitsList.Add(JpegHuffmanTable.StdACLuminance.Lengths);
                this.bitsList.Add(JpegHuffmanTable.StdDCChrominance.Lengths);
                this.bitsList.Add(JpegHuffmanTable.StdACChrominance.Lengths);
                this.val = new List<short[]>();
                this.val.Add(JpegHuffmanTable.StdDCLuminance.Values);
                this.val.Add(JpegHuffmanTable.StdACLuminance.Values);
                this.val.Add(JpegHuffmanTable.StdDCChrominance.Values);
                this.val.Add(JpegHuffmanTable.StdACChrominance.Values);
                this.initHuf();
            }
        }

        private void bufferIt(Stream outStream, int code, int size)
        {
            int num = code;
            int bufferPutBits = this.bufferPutBits;
            num &= (((int) 1) << size) - 1;
            bufferPutBits += size;
            num = num << (0x18 - bufferPutBits);
            num |= this.bufferPutBuffer;
            while (bufferPutBits >= 8)
            {
                int num3 = (num >> 0x10) & 0xff;
                outStream.WriteByte((byte) num3);
                if (num3 == 0xff)
                {
                    outStream.WriteByte(0);
                }
                num = num << 8;
                bufferPutBits -= 8;
            }
            this.bufferPutBuffer = num;
            this.bufferPutBits = bufferPutBits;
        }

        public int Decode(JPEGBinaryReader JPEGStream)
        {
            int index = 0;
            short num2 = (short) JPEGStream.ReadBits(1);
            while (num2 > this.maxcode[index])
            {
                index++;
                num2 = (short) (num2 << 1);
                num2 = (short) (num2 | ((short) JPEGStream.ReadBits(1)));
            }
            int num3 = this.huffval[num2 + this.valptr[index]];
            if (num3 < 0)
            {
                num3 = 0x100 + num3;
            }
            return num3;
        }

        public static int Extend(int diff, int t)
        {
            int num = ((int) 1) << (t - 1);
            if (diff < num)
            {
                num = (((int) (-1)) << t) + 1;
                diff += num;
            }
            return diff;
        }

        public void FlushBuffer(Stream outStream)
        {
            int num3;
            int bufferPutBuffer = this.bufferPutBuffer;
            int bufferPutBits = this.bufferPutBits;
            while (bufferPutBits >= 8)
            {
                num3 = (bufferPutBuffer >> 0x10) & 0xff;
                outStream.WriteByte((byte) num3);
                if (num3 == 0xff)
                {
                    outStream.WriteByte(0);
                }
                bufferPutBuffer = bufferPutBuffer << 8;
                bufferPutBits -= 8;
            }
            if (bufferPutBits > 0)
            {
                num3 = (bufferPutBuffer >> 0x10) & 0xff;
                outStream.WriteByte((byte) num3);
            }
        }

        private void GenerateCodeTable()
        {
            short index = 0;
            short num2 = this.huffsize[0];
            short num3 = 0;
            for (short i = 0; i < this.huffsize.Length; i = (short) (i + 1))
            {
                while (this.huffsize[index] == num2)
                {
                    this.huffcode[index] = num3;
                    num3 = (short) (num3 + 1);
                    index = (short) (index + 1);
                }
                num3 = (short) (num3 << 1);
                num2 = (short) (num2 + 1);
            }
        }

        private void GenerateDecoderTables()
        {
            short num = 0;
            for (int i = 0; i < 0x10; i++)
            {
                if (this.bits[i] != 0)
                {
                    this.valptr[i] = num;
                }
                for (int j = 0; j < this.bits[i]; j++)
                {
                    if ((this.huffcode[j + num] < this.mincode[i]) || (this.mincode[i] == -1))
                    {
                        this.mincode[i] = this.huffcode[j + num];
                    }
                    if (this.huffcode[j + num] > this.maxcode[i])
                    {
                        this.maxcode[i] = this.huffcode[j + num];
                    }
                }
                if (this.mincode[i] != -1)
                {
                    this.valptr[i] = (short) (this.valptr[i] - this.mincode[i]);
                }
                num = (short) (num + this.bits[i]);
            }
        }

        private void GenerateSizeTable()
        {
            short index = 0;
            for (short i = 0; i < this.bits.Length; i = (short) (i + 1))
            {
                for (short j = 0; j < this.bits[i]; j = (short) (j + 1))
                {
                    this.huffsize[index] = (short) (i + 1);
                    index = (short) (index + 1);
                }
            }
            this.lastk = index;
        }

        internal void HuffmanBlockEncoder(Stream outStream, int[] zigzag, int prec, int DCcode, int ACcode)
        {
            int num2;
            this.NumOfDCTables = 2;
            this.NumOfACTables = 2;
            int num = num2 = zigzag[0] - prec;
            if (num < 0)
            {
                num = -num;
                num2--;
            }
            int size = 0;
            while (num != 0)
            {
                size++;
                num = num >> 1;
            }
            this.bufferIt(outStream, this.DC_matrix[DCcode][size, 0], this.DC_matrix[DCcode][size, 1]);
            if (size != 0)
            {
                this.bufferIt(outStream, num2, size);
            }
            int num5 = 0;
            for (int i = 1; i < 0x40; i++)
            {
                num = zigzag[ZigZag.ZigZagMap[i]];
                if (num == 0)
                {
                    num5++;
                }
                else
                {
                    while (num5 > 15)
                    {
                        this.bufferIt(outStream, this.AC_matrix[ACcode][240, 0], this.AC_matrix[ACcode][240, 1]);
                        num5 -= 0x10;
                    }
                    num2 = num;
                    if (num < 0)
                    {
                        num = -num;
                        num2--;
                    }
                    size = 1;
                    while ((num = num >> 1) != 0)
                    {
                        size++;
                    }
                    int num6 = (num5 << 4) + size;
                    this.bufferIt(outStream, this.AC_matrix[ACcode][num6, 0], this.AC_matrix[ACcode][num6, 1]);
                    this.bufferIt(outStream, num2, size);
                    num5 = 0;
                }
            }
            if (num5 > 0)
            {
                this.bufferIt(outStream, this.AC_matrix[ACcode][0, 0], this.AC_matrix[ACcode][0, 1]);
            }
        }

        public void initHuf()
        {
            int num2;
            int num3;
            this.DC_matrix0 = new int[12, 2];
            this.DC_matrix1 = new int[12, 2];
            this.AC_matrix0 = new int[0xff, 2];
            this.AC_matrix1 = new int[0xff, 2];
            this.DC_matrix = new int[2][,];
            this.AC_matrix = new int[2][,];
            int[] numArray = new int[0x101];
            int[] numArray2 = new int[0x101];
            short[] lengths = JpegHuffmanTable.StdDCChrominance.Lengths;
            short[] numArray4 = JpegHuffmanTable.StdACChrominance.Lengths;
            short[] numArray5 = JpegHuffmanTable.StdDCLuminance.Lengths;
            short[] numArray6 = JpegHuffmanTable.StdACLuminance.Lengths;
            short[] values = JpegHuffmanTable.StdDCChrominance.Values;
            short[] numArray8 = JpegHuffmanTable.StdACChrominance.Values;
            short[] numArray9 = JpegHuffmanTable.StdDCLuminance.Values;
            short[] numArray10 = JpegHuffmanTable.StdACLuminance.Values;
            int index = 0;
            for (num2 = 0; num2 < 0x10; num2++)
            {
                num3 = 1;
                while (num3 <= lengths[num2])
                {
                    numArray[index++] = num2 + 1;
                    num3++;
                }
            }
            numArray[index] = 0;
            int num4 = index;
            int num6 = 0;
            int num5 = numArray[0];
            index = 0;
            while (numArray[index] != 0)
            {
                while (numArray[index] == num5)
                {
                    numArray2[index++] = num6;
                    num6++;
                }
                num6 = num6 << 1;
                num5++;
            }
            for (index = 0; index < num4; index++)
            {
                this.DC_matrix1[values[index], 0] = numArray2[index];
                this.DC_matrix1[values[index], 1] = numArray[index];
            }
            index = 0;
            for (num2 = 0; num2 < 0x10; num2++)
            {
                num3 = 1;
                while (num3 <= numArray4[num2])
                {
                    numArray[index++] = num2 + 1;
                    num3++;
                }
            }
            numArray[index] = 0;
            num4 = index;
            num6 = 0;
            num5 = numArray[0];
            index = 0;
            while (numArray[index] != 0)
            {
                while (numArray[index] == num5)
                {
                    numArray2[index++] = num6;
                    num6++;
                }
                num6 = num6 << 1;
                num5++;
            }
            for (index = 0; index < num4; index++)
            {
                this.AC_matrix1[numArray8[index], 0] = numArray2[index];
                this.AC_matrix1[numArray8[index], 1] = numArray[index];
            }
            index = 0;
            for (num2 = 0; num2 < 0x10; num2++)
            {
                num3 = 1;
                while (num3 <= numArray5[num2])
                {
                    numArray[index++] = num2 + 1;
                    num3++;
                }
            }
            numArray[index] = 0;
            num4 = index;
            num6 = 0;
            num5 = numArray[0];
            index = 0;
            while (numArray[index] != 0)
            {
                while (numArray[index] == num5)
                {
                    numArray2[index++] = num6;
                    num6++;
                }
                num6 = num6 << 1;
                num5++;
            }
            for (index = 0; index < num4; index++)
            {
                this.DC_matrix0[numArray9[index], 0] = numArray2[index];
                this.DC_matrix0[numArray9[index], 1] = numArray[index];
            }
            index = 0;
            for (num2 = 0; num2 < 0x10; num2++)
            {
                for (num3 = 1; num3 <= numArray6[num2]; num3++)
                {
                    numArray[index++] = num2 + 1;
                }
            }
            numArray[index] = 0;
            num4 = index;
            num6 = 0;
            num5 = numArray[0];
            index = 0;
            while (numArray[index] != 0)
            {
                while (numArray[index] == num5)
                {
                    numArray2[index++] = num6;
                    num6++;
                }
                num6 = num6 << 1;
                num5++;
            }
            for (int i = 0; i < num4; i++)
            {
                this.AC_matrix0[numArray10[i], 0] = numArray2[i];
                this.AC_matrix0[numArray10[i], 1] = numArray[i];
            }
            this.DC_matrix[0] = this.DC_matrix0;
            this.DC_matrix[1] = this.DC_matrix1;
            this.AC_matrix[0] = this.AC_matrix0;
            this.AC_matrix[1] = this.AC_matrix1;
        }
    }
}

