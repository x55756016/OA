namespace FluxJpeg.Core.Encoder
{
    using FluxJpeg.Core;
    using FluxJpeg.Core.IO;
    using System;
    using System.IO;
    using System.Runtime.CompilerServices;

    public class JpegEncoder
    {
        private DCT _dct;
        private int _height;
        private HuffmanTable _huf;
        private DecodedJpeg _input;
        private Stream _outStream;
        private JpegEncodeProgressChangedArgs _progress;
        private int _quality;
        private int _width;
        private static readonly int[] ACtableNumber;
        private const int Ah = 0;
        private const int Al = 0;
        private static readonly int[] CompID = new int[] { 1, 2, 3 };
        private static readonly int[] DCtableNumber;
        private static readonly int[] HsampFactor = new int[] { 1, 1, 1 };
        private static readonly int[] QtableNumber;
        private const int Se = 0x3f;
        private const int Ss = 0;
        private static readonly int[] VsampFactor = new int[] { 1, 1, 1 };

        public event EventHandler<JpegEncodeProgressChangedArgs> EncodeProgressChanged;

        static JpegEncoder()
        {
            int[] numArray = new int[3];
            numArray[1] = 1;
            numArray[2] = 1;
            QtableNumber = numArray;
            numArray = new int[3];
            numArray[1] = 1;
            numArray[2] = 1;
            DCtableNumber = numArray;
            numArray = new int[3];
            numArray[1] = 1;
            numArray[2] = 1;
            ACtableNumber = numArray;
        }

        public JpegEncoder(DecodedJpeg decodedJpeg, int quality, Stream outStream)
        {
            this._input = decodedJpeg;
            this._input.Image.ChangeColorSpace(ColorSpace.YCbCr);
            this._quality = quality;
            this._height = this._input.Image.Height;
            this._width = this._input.Image.Width;
            this._outStream = outStream;
            this._dct = new DCT(this._quality);
            this._huf = new HuffmanTable(null);
        }

        public JpegEncoder(Image image, int quality, Stream outStream) : this(new DecodedJpeg(image), quality, outStream)
        {
        }

        internal void CompressTo(Stream outStream)
        {
            int num = 0;
            int num2 = 0;
            int num3 = 0;
            int num4 = 0;
            int num5 = 0;
            int num6 = 0;
            byte[,] buffer = null;
            float[,] input = new float[8, 8];
            float[,] inputData = new float[8, 8];
            int[] zigzag = new int[0x40];
            int[] numArray4 = new int[this._input.Image.ComponentCount];
            int num12 = 0;
            int num13 = 0;
            int num14 = ((this._width % 8) != 0) ? (((int) (Math.Floor(((double) this._width) / 8.0) + 1.0)) * 8) : this._width;
            int num15 = ((this._height % 8) != 0) ? (((int) (Math.Floor(((double) this._height) / 8.0) + 1.0)) * 8) : this._height;
            int index = 0;
            while (index < this._input.Image.ComponentCount)
            {
                num14 = Math.Min(num14, this._input.BlockWidth[index]);
                num15 = Math.Min(num15, this._input.BlockHeight[index]);
                index++;
            }
            int num8 = 0;
            for (num3 = 0; num3 < num15; num3++)
            {
                this._progress.EncodeProgress = ((double) num3) / ((double) num15);
                if (this.EncodeProgressChanged != null)
                {
                    this.EncodeProgressChanged(this, this._progress);
                }
                for (num4 = 0; num4 < num14; num4++)
                {
                    num8 = num4 * 8;
                    int num9 = num3 * 8;
                    for (index = 0; index < this._input.Image.ComponentCount; index++)
                    {
                        num12 = this._input.BlockWidth[index];
                        num13 = this._input.BlockHeight[index];
                        buffer = this._input.Image.Raster[index];
                        for (num = 0; num < this._input.VsampFactor[index]; num++)
                        {
                            for (num2 = 0; num2 < this._input.HsampFactor[index]; num2++)
                            {
                                int num10 = num2 * 8;
                                int num11 = num * 8;
                                for (num5 = 0; num5 < 8; num5++)
                                {
                                    int num16 = (num9 + num11) + num5;
                                    if (num16 >= this._height)
                                    {
                                        break;
                                    }
                                    for (num6 = 0; num6 < 8; num6++)
                                    {
                                        int num17 = (num8 + num10) + num6;
                                        if (num17 >= this._width)
                                        {
                                            break;
                                        }
                                        input[num5, num6] = buffer[num17, num16];
                                    }
                                }
                                inputData = this._dct.FastFDCT(input);
                                zigzag = this._dct.QuantizeBlock(inputData, QtableNumber[index]);
                                this._huf.HuffmanBlockEncoder(outStream, zigzag, numArray4[index], DCtableNumber[index], ACtableNumber[index]);
                                numArray4[index] = zigzag[0];
                            }
                        }
                    }
                }
            }
            this._huf.FlushBuffer(outStream);
        }

        public void Encode()
        {
            this._progress = new JpegEncodeProgressChangedArgs();
          //  this.WriteHeaders();
            this.CompressTo(this._outStream);
            this.WriteMarker(new byte[] { 0xff, 0xd9 });
            this._progress.EncodeProgress = 1.0;
            if (this.EncodeProgressChanged != null)
            {
                this.EncodeProgressChanged(this, this._progress);
            }
            this._outStream.Flush();
        }

        private void WriteArray(byte[] data)
        {
            int count = (((data[2] & 0xff) << 8) + (data[3] & 0xff)) + 2;
            this._outStream.Write(data, 0, count);
        }

        //internal void WriteHeaders()
        //{
        //    int num;
        //    int num2;
        //    byte[] data = new byte[] { 0xff, 0xd8 };
        //    this.WriteMarker(data);
        //    if (!this._input.HasJFIF)
        //    {
        //        byte[] buffer2 = new byte[] { 
        //            0xff, 0xe0, 0, 0x10, 0x4a, 70, 0x49, 70, 0, 1, 0, 0, 0, 1, 0, 1, 
        //            0, 0
        //         };
        //        this.WriteArray(buffer2);
        //    }
        //    FluxJpeg.Core.IO.BinaryWriter writer = new FluxJpeg.Core.IO.BinaryWriter(this._outStream);
        //    foreach (JpegHeader header in this._input.MetaHeaders)
        //    {
        //        writer.Write((byte) 0xff);
        //        writer.Write(header.Marker);
        //        writer.Write((short) (header.Data.Length + 2));
        //        writer.Write(header.Data);
        //    }
        //    byte[] buffer3 = new byte[0x86];
        //    buffer3[0] = 0xff;
        //    buffer3[1] = 0xdb;
        //    buffer3[2] = 0;
        //    buffer3[3] = 0x84;
        //    int num4 = 4;
        //    for (num = 0; num < 2; num++)
        //    {
        //        buffer3[num4++] = (byte) num;
        //        int[] numArray = this._dct.quantum[num];
        //        num2 = 0;
        //        while (num2 < 0x40)
        //        {
        //            buffer3[num4++] = (byte) numArray[ZigZag.ZigZagMap[num2]];
        //            num2++;
        //        }
        //    }
        //    this.WriteArray(buffer3);
        //    byte[] buffer4 = new byte[0x13];
        //    buffer4[0] = 0xff;
        //    buffer4[1] = 0xc0;
        //    buffer4[2] = 0;
        //    buffer4[3] = 0x11;
        //    buffer4[4] = (byte) this._input.Precision;
        //    buffer4[5] = (byte) ((this._input.Image.Height >> 8) & 0xff);
        //    buffer4[6] = (byte) (this._input.Image.Height & 0xff);
        //    buffer4[7] = (byte) ((this._input.Image.Width >> 8) & 0xff);
        //    buffer4[8] = (byte) (this._input.Image.Width & 0xff);
        //    buffer4[9] = (byte) this._input.Image.ComponentCount;
        //    int num3 = 10;
        //    for (num = 0; num < buffer4[9]; num++)
        //    {
        //        buffer4[num3++] = (byte) CompID[num];
        //        buffer4[num3++] = (byte) ((this._input.HsampFactor[num] << 4) + this._input.VsampFactor[num]);
        //        buffer4[num3++] = (byte) QtableNumber[num];
        //    }
        //    this.WriteArray(buffer4);
        //    num3 = 4;
        //    int length = 4;
        //    byte[] sourceArray = new byte[0x11];
        //    byte[] buffer8 = new byte[4];
        //    buffer8[0] = 0xff;
        //    buffer8[1] = 0xc4;
        //    for (num = 0; num < 4; num++)
        //    {
        //        int num5 = 0;
        //        byte num9 = (num == 0) ? ((byte) 0) : ((num == 1) ? ((byte) 0x10) : ((num == 2) ? ((byte) 1) : ((byte) 0x11)));
        //        sourceArray[num3++ - length] = num9;
        //        num2 = 0;
        //        while (num2 < 0x10)
        //        {
        //            int num6 = this._huf.bitsList[num][num2];
        //            sourceArray[num3++ - length] = (byte) num6;
        //            num5 += num6;
        //            num2++;
        //        }
        //        int num8 = num3;
        //        byte[] buffer6 = new byte[num5];
        //        for (num2 = 0; num2 < num5; num2++)
        //        {
        //            buffer6[num3++ - num8] = (byte) this._huf.val[num][num2];
        //        }
        //        byte[] destinationArray = new byte[num3];
        //        Array.Copy(buffer8, 0, destinationArray, 0, length);
        //        Array.Copy(sourceArray, 0, destinationArray, length, 0x11);
        //        Array.Copy(buffer6, 0, destinationArray, length + 0x11, num5);
        //        buffer8 = destinationArray;
        //        length = num3;
        //    }
        //    buffer8[2] = (byte) (((num3 - 2) >> 8) & 0xff);
        //    buffer8[3] = (byte) ((num3 - 2) & 0xff);
        //    this.WriteArray(buffer8);
        //    byte[] buffer9 = new byte[14];
        //    buffer9[0] = 0xff;
        //    buffer9[1] = 0xda;
        //    buffer9[2] = 0;
        //    buffer9[3] = 12;
        //    buffer9[4] = (byte) this._input.Image.ComponentCount;
        //    num3 = 5;
        //    for (num = 0; num < buffer9[4]; num++)
        //    {
        //        buffer9[num3++] = (byte) CompID[num];
        //        buffer9[num3++] = (byte) ((DCtableNumber[num] << 4) + ACtableNumber[num]);
        //    }
        //    buffer9[num3++] = 0;
        //    buffer9[num3++] = 0x3f;
        //    buffer9[num3++] = 0;
        //    this.WriteArray(buffer9);
        //}

        private void WriteMarker(byte[] data)
        {
            this._outStream.Write(data, 0, 2);
        }
    }
}

