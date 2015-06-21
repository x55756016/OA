namespace FluxJpeg.Core.Decoder
{
    using FluxJpeg.Core;
    using FluxJpeg.Core.IO;
    using System;
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;

    internal class JpegComponent
    {
        private DCT _dct = new DCT();
        private QuantizeDel _quant = null;
        public HuffmanTable ACTable;
        public byte component_id;
        public HuffmanTable DCTable;
        public DecodeFunction Decode;
        public byte factorH;
        public byte factorV;
        public int height = 0;
        private JpegScan parent;
        public float previousDC = 0f;
        public byte quant_id;
        private int[] quantizationTable;
        private List<float[,][]> scanData = new List<float[,][]>();
        private List<byte[,]> scanDecoded = new List<byte[,]>();
        private float[,][] scanMCUs = null;
        public int spectralEnd;
        public int spectralStart;
        public int successiveLow;
        public int width = 0;

        public JpegComponent(JpegScan parentScan, byte id, byte factorHorizontal, byte factorVertical, byte quantizationID, byte colorMode)
        {
            this.parent = parentScan;
            if (colorMode == JPEGFrame.JPEG_COLOR_YCbCr)
            {
                if (id == 1)
                {
                    this.ACTable = new HuffmanTable(JpegHuffmanTable.StdACLuminance);
                    this.DCTable = new HuffmanTable(JpegHuffmanTable.StdDCLuminance);
                }
                else
                {
                    this.ACTable = new HuffmanTable(JpegHuffmanTable.StdACChrominance);
                    this.DCTable = new HuffmanTable(JpegHuffmanTable.StdACLuminance);
                }
            }
            this.component_id = id;
            this.factorH = factorHorizontal;
            this.factorV = factorVertical;
            this.quant_id = quantizationID;
        }

        internal void decode_ac_coefficients(JPEGBinaryReader JPEGStream, float[] zz)
        {
            for (int i = 1; i < 0x40; i++)
            {
                int t = this.ACTable.Decode(JPEGStream);
                int num3 = t >> 4;
                t &= 15;
                if (t != 0)
                {
                    i += num3;
                    zz[i] = HuffmanTable.Extend(JPEGStream.ReadBits(t), t);
                }
                else
                {
                    if (num3 != 15)
                    {
                        break;
                    }
                    i += 15;
                }
            }
        }

        public float decode_dc_coefficient(JPEGBinaryReader JPEGStream)
        {
            int n = this.DCTable.Decode(JPEGStream);
            float num2 = JPEGStream.ReadBits(n);
            num2 = HuffmanTable.Extend((int) num2, n);
            num2 = this.previousDC + num2;
            this.previousDC = num2;
            return num2;
        }

        public void DecodeACFirst(JPEGBinaryReader stream, float[] zz)
        {
            if (stream.eob_run > 0)
            {
                stream.eob_run--;
            }
            else
            {
                for (int i = this.spectralStart; i <= this.spectralEnd; i++)
                {
                    int t = this.ACTable.Decode(stream);
                    int n = t >> 4;
                    t &= 15;
                    if (t != 0)
                    {
                        i += n;
                        t = HuffmanTable.Extend(stream.ReadBits(t), t);
                        zz[i] = t << this.successiveLow;
                    }
                    else
                    {
                        if (n != 15)
                        {
                            stream.eob_run = ((int) 1) << n;
                            if (n != 0)
                            {
                                stream.eob_run += stream.ReadBits(n);
                            }
                            stream.eob_run--;
                            break;
                        }
                        i += 15;
                    }
                }
            }
        }

        public void DecodeACRefine(JPEGBinaryReader stream, float[] dest)
        {
            int num = ((int) 1) << this.successiveLow;
            int num2 = ((int) (-1)) << this.successiveLow;
            int spectralStart = this.spectralStart;
            if (stream.eob_run == 0)
            {
                while (spectralStart <= this.spectralEnd)
                {
                    int num4 = this.ACTable.Decode(stream);
                    int n = num4 >> 4;
                    num4 &= 15;
                    if (num4 != 0)
                    {
                        if (num4 != 1)
                        {
                            throw new Exception("Decode Error");
                        }
                        if (stream.ReadBits(1) == 1)
                        {
                            num4 = num;
                        }
                        else
                        {
                            num4 = num2;
                        }
                    }
                    else if (n != 15)
                    {
                        stream.eob_run = ((int) 1) << n;
                        if (n > 0)
                        {
                            stream.eob_run += stream.ReadBits(n);
                        }
                        break;
                    }
                    do
                    {
                        if (dest[spectralStart] != 0f)
                        {
                            if ((stream.ReadBits(1) == 1) && ((((int) dest[spectralStart]) & num) == 0))
                            {
                                if (dest[spectralStart] >= 0f)
                                {
                                    dest[spectralStart] += num;
                                }
                                else
                                {
                                    dest[spectralStart] += num2;
                                }
                            }
                        }
                        else if (--n < 0)
                        {
                            break;
                        }
                        spectralStart++;
                    }
                    while (spectralStart <= this.spectralEnd);
                    if ((num4 != 0) && (spectralStart < 0x40))
                    {
                        dest[spectralStart] = num4;
                    }
                    spectralStart++;
                }
            }
            if (stream.eob_run > 0)
            {
                while (spectralStart <= this.spectralEnd)
                {
                    if (((dest[spectralStart] != 0f) && (stream.ReadBits(1) == 1)) && ((((int) dest[spectralStart]) & num) == 0))
                    {
                        if (dest[spectralStart] >= 0f)
                        {
                            dest[spectralStart] += num;
                        }
                        else
                        {
                            dest[spectralStart] += num2;
                        }
                    }
                    spectralStart++;
                }
                stream.eob_run--;
            }
        }

        public void DecodeBaseline(JPEGBinaryReader stream, float[] dest)
        {
            float num = this.decode_dc_coefficient(stream);
            this.decode_ac_coefficients(stream, dest);
            dest[0] = num;
        }

        public void DecodeDCFirst(JPEGBinaryReader stream, float[] dest)
        {
            float[] numArray = new float[0x40];
            int t = this.DCTable.Decode(stream);
            t = HuffmanTable.Extend(stream.ReadBits(t), t);
            t = ((int) this.previousDC) + t;
            this.previousDC = t;
            dest[0] = t << this.successiveLow;
        }

        public void DecodeDCRefine(JPEGBinaryReader stream, float[] dest)
        {
            if (stream.ReadBits(1) == 1)
            {
                dest[0] = ((int) dest[0]) | (((int) 1) << this.successiveLow);
            }
        }

        public void DecodeMCU(JPEGBinaryReader jpegReader, int i, int j)
        {
            this.Decode(jpegReader, this.scanMCUs[i, j]);
        }

        private QuantizeDel EmitQuantize()
        {
            Type[] parameterTypes = new Type[] { typeof(float[]) };
            DynamicMethod method = new DynamicMethod("Quantize", null, parameterTypes);
            ILGenerator iLGenerator = method.GetILGenerator();
            for (int i = 0; i < this.quantizationTable.Length; i++)
            {
                float arg = this.quantizationTable[i];
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Ldc_I4_S, (short) i);
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Ldc_I4_S, (short) i);
                iLGenerator.Emit(OpCodes.Ldelem_R4);
                iLGenerator.Emit(OpCodes.Ldc_R4, arg);
                iLGenerator.Emit(OpCodes.Mul);
                iLGenerator.Emit(OpCodes.Stelem_R4);
            }
            iLGenerator.Emit(OpCodes.Ret);
            return (QuantizeDel) method.CreateDelegate(typeof(QuantizeDel));
        }

        public void idctData()
        {
            float[] output = new float[0x40];
            float[] input = null;
            for (int i = 0; i < this.scanData.Count; i++)
            {
                for (int j = 0; j < this.factorV; j++)
                {
                    for (int k = 0; k < this.factorH; k++)
                    {
                        input = this.scanData[i][k, j];
                        ZigZag.UnZigZag(input, output);
                        this.scanDecoded.Add(this._dct.FastIDCT(output));
                    }
                }
            }
        }

        public void padMCU(int index, int length)
        {
            this.scanMCUs = new float[this.factorH, this.factorV][];
            for (int i = 0; i < length; i++)
            {
                if (this.scanData.Count < (index + length))
                {
                    for (int j = 0; j < this.factorH; j++)
                    {
                        for (int k = 0; k < this.factorV; k++)
                        {
                            this.scanMCUs[j, k] = (float[]) this.scanData[index - 1][j, k].Clone();
                        }
                    }
                    this.scanData.Add(this.scanMCUs);
                }
            }
        }

        public void quantizeData()
        {
            for (int i = 0; i < this.scanData.Count; i++)
            {
                for (int j = 0; j < this.factorV; j++)
                {
                    for (int k = 0; k < this.factorH; k++)
                    {
                        this._quant(this.scanData[i][k, j]);
                    }
                }
            }
        }

        public void resetInterval()
        {
            this.previousDC = 0f;
        }

        public void scaleByFactors(BlockUpsamplingMode mode)
        {
            int factorUpV = this.factorUpV;
            int factorUpH = this.factorUpH;
            if ((factorUpV != 1) || (factorUpH != 1))
            {
                for (int i = 0; i < this.scanDecoded.Count; i++)
                {
                    int num8;
                    int num9;
                    int num11;
                    byte[,] buffer = this.scanDecoded[i];
                    int length = buffer.GetLength(0);
                    int num5 = buffer.GetLength(1);
                    int num6 = length * factorUpV;
                    int num7 = num5 * factorUpH;
                    byte[,] buffer2 = new byte[num6, num7];
                    switch (mode)
                    {
                        case BlockUpsamplingMode.BoxFilter:
                            num8 = 0;
                            goto Label_00C4;

                        case BlockUpsamplingMode.Interpolate:
                            num8 = 0;
                            goto Label_018E;

                        default:
                            throw new ArgumentException("Upsampling mode not supported.");
                    }
                Label_0082:
                    num9 = num8 / factorUpH;
                    int num10 = 0;
                    while (num10 < num6)
                    {
                        num11 = num10 / factorUpV;
                        buffer2[num10, num8] = buffer[num11, num9];
                        num10++;
                    }
                    num8++;
                Label_00C4:
                    if (num8 < num7)
                    {
                        goto Label_0082;
                    }
                    goto Label_01AA;
                Label_00DD:
                    num10 = 0;
                    while (num10 < num6)
                    {
                        int num12 = 0;
                        for (int j = 0; j < factorUpH; j++)
                        {
                            num9 = (num8 + j) / factorUpH;
                            if (num9 >= num5)
                            {
                                num9 = num5 - 1;
                            }
                            for (int k = 0; k < factorUpV; k++)
                            {
                                num11 = (num10 + k) / factorUpV;
                                if (num11 >= length)
                                {
                                    num11 = length - 1;
                                }
                                num12 += buffer[num11, num9];
                            }
                        }
                        buffer2[num10, num8] = (byte) (num12 / (factorUpH * factorUpV));
                        num10++;
                    }
                    num8++;
                Label_018E:
                    if (num8 < num7)
                    {
                        goto Label_00DD;
                    }
                Label_01AA:
                    this.scanDecoded[i] = buffer2;
                }
            }
        }

        public void setACTable(JpegHuffmanTable table)
        {
            this.ACTable = new HuffmanTable(table);
        }

        public void SetBlock(int idx)
        {
            if (this.scanData.Count < idx)
            {
                throw new Exception("Invalid block ID.");
            }
            if (this.scanData.Count == idx)
            {
                this.scanMCUs = new float[this.factorH, this.factorV][];
                for (int i = 0; i < this.factorH; i++)
                {
                    for (int j = 0; j < this.factorV; j++)
                    {
                        this.scanMCUs[i, j] = new float[0x40];
                    }
                }
                this.scanData.Add(this.scanMCUs);
            }
            else
            {
                this.scanMCUs = this.scanData[idx];
            }
        }

        public void setDCTable(JpegHuffmanTable table)
        {
            this.DCTable = new HuffmanTable(table);
        }

        public void writeBlock(byte[][,] raster, byte[,] data, int compIndex, int x, int y)
        {
            int length = raster[0].GetLength(0);
            int num2 = raster[0].GetLength(1);
            byte[,] buffer = raster[compIndex];
            int num3 = data.GetLength(0);
            if ((y + num3) > num2)
            {
                num3 = num2 - y;
            }
            int num4 = data.GetLength(1);
            if ((x + num4) > length)
            {
                num4 = length - x;
            }
            for (int i = 0; i < num3; i++)
            {
                for (int j = 0; j < num4; j++)
                {
                    buffer[x + j, y + i] = data[i, j];
                }
            }
        }

        private void writeBlockScaled(byte[][,] raster, byte[,] blockdata, int compIndex, int x, int y, BlockUpsamplingMode mode)
        {
            int num11;
            int num12;
            int length = raster[0].GetLength(0);
            int num2 = raster[0].GetLength(1);
            int factorUpV = this.factorUpV;
            int factorUpH = this.factorUpH;
            int num5 = blockdata.GetLength(0);
            int num6 = blockdata.GetLength(1);
            int num7 = num5 * factorUpV;
            int num8 = num6 * factorUpH;
            byte[,] buffer = raster[compIndex];
            int num9 = num7;
            if ((y + num9) > num2)
            {
                num9 = num2 - y;
            }
            int num10 = num8;
            if ((x + num10) > length)
            {
                num10 = length - x;
            }
            if (mode != BlockUpsamplingMode.BoxFilter)
            {
                throw new ArgumentException("Upsampling mode not supported.");
            }
            if ((factorUpV == 1) && (factorUpH == 1))
            {
                for (num11 = 0; num11 < num10; num11++)
                {
                    num12 = 0;
                    while (num12 < num9)
                    {
                        buffer[num11 + x, y + num12] = blockdata[num12, num11];
                        num12++;
                    }
                }
            }
            else
            {
                int num13;
                int num15;
                if ((((factorUpH == 2) && (factorUpV == 2)) && (num10 == num8)) && (num9 == num7))
                {
                    num13 = 0;
                    while (num13 < num6)
                    {
                        int num14 = (num13 * 2) + x;
                        num15 = 0;
                        while (num15 < num5)
                        {
                            byte num16 = blockdata[num15, num13];
                            int num17 = (num15 * 2) + y;
                            buffer[num14, num17] = num16;
                            buffer[num14, num17 + 1] = num16;
                            buffer[num14 + 1, num17] = num16;
                            buffer[num14 + 1, num17 + 1] = num16;
                            num15++;
                        }
                        num13++;
                    }
                }
                else
                {
                    for (num11 = 0; num11 < num10; num11++)
                    {
                        num13 = num11 / factorUpH;
                        for (num12 = 0; num12 < num9; num12++)
                        {
                            num15 = num12 / factorUpV;
                            buffer[num11 + x, y + num12] = blockdata[num15, num13];
                        }
                    }
                }
            }
        }

        public void writeDataScaled(byte[][,] raster, int componentIndex, BlockUpsamplingMode mode)
        {
            int x = 0;
            int y = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            int length = raster[0].GetLength(0);
            int num7 = raster[0].GetLength(1);
            while (num5 < this.scanDecoded.Count)
            {
                int num8 = 0;
                int num9 = 0;
                if (x >= length)
                {
                    x = 0;
                    y += num4;
                }
                for (int i = 0; i < this.factorV; i++)
                {
                    num8 = 0;
                    for (int j = 0; j < this.factorH; j++)
                    {
                        byte[,] blockdata = this.scanDecoded[num5++];
                        this.writeBlockScaled(raster, blockdata, componentIndex, x, y, mode);
                        num8 += blockdata.GetLength(1) * this.factorUpH;
                        x += blockdata.GetLength(1) * this.factorUpH;
                        num9 = blockdata.GetLength(0) * this.factorUpV;
                    }
                    y += num9;
                    x -= num8;
                    num3 += num9;
                }
                y -= num3;
                num4 = num3;
                num3 = 0;
                x += num8;
            }
        }

        public int BlockCount
        {
            get
            {
                return this.scanData.Count;
            }
        }

        private int factorUpH
        {
            get
            {
                return (this.parent.MaxH / this.factorH);
            }
        }

        private int factorUpV
        {
            get
            {
                return (this.parent.MaxV / this.factorV);
            }
        }

        public int[] QuantizationTable
        {
            set
            {
                this.quantizationTable = value;
                this._quant = this.EmitQuantize();
            }
        }

        internal delegate void DecodeFunction(JPEGBinaryReader jpegReader, float[] zigzagMCU);

        private delegate void QuantizeDel(float[] arr);
    }
}

