namespace FluxJpeg.Core.Decoder
{
    using FluxJpeg.Core;
    using FluxJpeg.Core.IO;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Text;

    public class JpegDecoder
    {
        [CompilerGenerated]
       // private FluxJpeg.Core.Decoder.BlockUpsamplingMode <BlockUpsamplingMode>k__BackingField;
        private JpegHuffmanTable[] acTables = new JpegHuffmanTable[4];
        private JpegHuffmanTable[] dcTables = new JpegHuffmanTable[4];
        private JpegDecodeProgressChangedArgs DecodeProgress = new JpegDecodeProgressChangedArgs();
        private int height;
        private Image image;
        internal static short JFIF_FIXED_LENGTH = 0x10;
        internal static short JFXX_FIXED_LENGTH = 8;
        private List<JPEGFrame> jpegFrames = new List<JPEGFrame>();
        private JPEGBinaryReader jpegReader;
        internal const byte MAJOR_VERSION = 1;
        private byte majorVersion;
        private byte marker;
        internal const byte MINOR_VERSION = 2;
        private byte minorVersion;
        private bool progressive = false;
        public static long ProgressUpdateByteInterval = 100L;
        private JpegQuantizationTable[] qTables = new JpegQuantizationTable[4];
        private byte[] thumbnail;
        private UnitType Units;
        private int width;
        private ushort XDensity;
        private byte Xthumbnail;
        private ushort YDensity;
        private byte Ythumbnail;

        public event EventHandler<JpegDecodeProgressChangedArgs> DecodeProgressChanged;

        public JpegDecoder(Stream input)
        {
            this.jpegReader = new JPEGBinaryReader(input);
            if (this.jpegReader.GetNextMarker() != 0xd8)
            {
                throw new Exception("Failed to find SOI marker.");
            }
        }

        //public DecodedJpeg Decode()
        //{
        //    JPEGFrame frame = null;
        //    int num2;
        //    Func<double, double> func2 = null;
        //    bool flag3;
        //    int resetInterval = 0;
        //    bool flag = false;
        //    bool flag2 = false;
        //    List<JpegHeader> metaHeaders = new List<JpegHeader>();
        //    goto Label_09C3;
        //Label_03AF:
        //    num2 = 0;
        //    while (num2 < frame.ComponentCount)
        //    {
        //        byte componentID = this.jpegReader.ReadByte();
        //        byte num4 = this.jpegReader.ReadByte();
        //        byte quantizationTableID = this.jpegReader.ReadByte();
        //        byte sampleHFactor = (byte)(num4 >> 4);
        //        byte sampleVFactor = (byte)(num4 & 15);
        //        frame.AddComponent(componentID, sampleHFactor, sampleVFactor, quantizationTableID);
        //        num2++;
        //    }
        //Label_0998:
        //    if (flag)
        //    {
        //        flag = false;
        //    }
        //    else
        //    {
        //        try
        //        {
        //            this.marker = this.jpegReader.GetNextMarker();
        //        }
        //        catch (EndOfStreamException)
        //        {
        //            return new DecodedJpeg(this.image, metaHeaders);
        //        }
        //    }
        //Label_09C3:
        //    flag3 = true;
        //    if (this.DecodeProgress.Abort)
        //    {
        //        return null;
        //    }
        //    switch (this.marker)
        //    {
        //        case 0xc0:
        //        case 0xc2:
        //            this.progressive = this.marker == 0xc2;
        //            this.jpegFrames.Add(new JPEGFrame());
        //            frame = this.jpegFrames[this.jpegFrames.Count - 1];
        //            frame.ProgressUpdateMethod = new Action<long>(this.UpdateStreamProgress);
        //            this.jpegReader.ReadShort();
        //            frame.setPrecision(this.jpegReader.ReadByte());
        //            frame.ScanLines = this.jpegReader.ReadShort();
        //            frame.SamplesPerLine = this.jpegReader.ReadShort();
        //            frame.ComponentCount = this.jpegReader.ReadByte();
        //            this.DecodeProgress.Height = frame.Height;
        //            this.DecodeProgress.Width = frame.Width;
        //            this.DecodeProgress.SizeReady = true;
        //            if (this.DecodeProgressChanged == null)
        //            {
        //                goto Label_03AF;
        //            }
        //            this.DecodeProgressChanged(this, this.DecodeProgress);
        //            if (!this.DecodeProgress.Abort)
        //            {
        //                goto Label_03AF;
        //            }
        //            return null;

        //        case 0xc1:
        //        case 0xc3:
        //        case 0xc5:
        //        case 0xc6:
        //        case 0xc7:
        //        case 0xc9:
        //        case 0xca:
        //        case 0xcb:
        //        case 0xcd:
        //        case 0xce:
        //        case 0xcf:
        //            throw new NotSupportedException("Unsupported codec type.");

        //        case 0xc4:
        //            {
        //                int num8 = this.jpegReader.ReadShort() - 2;
        //                int num9 = num8;
        //                while (num9 > 0)
        //                {
        //                    byte num10 = this.jpegReader.ReadByte();
        //                    byte num11 = (byte)(num10 >> 4);
        //                    byte index = (byte)(num10 & 15);
        //                    short[] lengths = new short[0x10];
        //                    for (num2 = 0; num2 < lengths.Length; num2++)
        //                    {
        //                        lengths[num2] = this.jpegReader.ReadByte();
        //                    }
        //                    int num13 = 0;
        //                    for (num2 = 0; num2 < 0x10; num2++)
        //                    {
        //                        num13 += lengths[num2];
        //                    }
        //                    num9 -= num13 + 0x11;
        //                    short[] values = new short[num13];
        //                    for (num2 = 0; num2 < values.Length; num2++)
        //                    {
        //                        values[num2] = this.jpegReader.ReadByte();
        //                    }
        //                    if (num11 == HuffmanTable.JPEG_DC_TABLE)
        //                    {
        //                        this.dcTables[index] = new JpegHuffmanTable(lengths, values);
        //                    }
        //                    else if (num11 == HuffmanTable.JPEG_AC_TABLE)
        //                    {
        //                        this.acTables[index] = new JpegHuffmanTable(lengths, values);
        //                    }
        //                }
        //                goto Label_0998;
        //            }
        //        case 200:
        //        case 0xcc:
        //        case 0xd0:
        //        case 0xd1:
        //        case 210:
        //        case 0xd3:
        //        case 0xd4:
        //        case 0xd5:
        //        case 0xd6:
        //        case 0xd7:
        //        case 0xd8:
        //        case 0xde:
        //        case 0xdf:
        //        case 240:
        //        case 0xf1:
        //        case 0xf2:
        //        case 0xf3:
        //        case 0xf4:
        //        case 0xf5:
        //        case 0xf6:
        //        case 0xf7:
        //        case 0xf8:
        //        case 0xf9:
        //        case 250:
        //        case 0xfb:
        //        case 0xfc:
        //        case 0xfd:
        //            goto Label_0998;

        //        case 0xd9:
        //            {
        //                ColorModel model;
        //                ColorModel model4;
        //                if (this.jpegFrames.Count == 0)
        //                {
        //                    throw new NotSupportedException("No JPEG frames could be located.");
        //                }
        //                if (this.jpegFrames.Count != 1)
        //                {
        //                    throw new NotSupportedException("Unsupported Codec Type: Hierarchial JPEG");
        //                }
        //                byte[][,] raster = Image.CreateRaster(frame.Width, frame.Height, frame.ComponentCount);
        //                IList<JpegComponent> components = frame.Scan.Components;
        //                int stepsTotal = components.Count * 3;
        //                int num27 = 0;
        //                for (num2 = 0; num2 < components.Count; num2++)
        //                {
        //                    JpegComponent component = components[num2];
        //                    component.QuantizationTable = this.qTables[component.quant_id].Table;
        //                    component.quantizeData();
        //                    this.UpdateProgress(++num27, stepsTotal);
        //                    component.idctData();
        //                    this.UpdateProgress(++num27, stepsTotal);
        //                    component.writeDataScaled(raster, num2, this.BlockUpsamplingMode);
        //                    this.UpdateProgress(++num27, stepsTotal);
        //                    component = null;
        //                    GC.Collect();
        //                }
        //                if (frame.ComponentCount == 1)
        //                {
        //                    model4 = new ColorModel();
        //                    ColorModel model2 = model4;
        //                    model2.colorspace = ColorSpace.Gray;
        //                    model2.Opaque = true;
        //                    model = model2;
        //                    this.image = new Image(model, raster);
        //                }
        //                else
        //                {
        //                    if (frame.ComponentCount != 3)
        //                    {
        //                        throw new NotSupportedException("Unsupported Color Mode: 4 Component Color Mode found.");
        //                    }
        //                    model4 = new ColorModel();
        //                    ColorModel model3 = model4;
        //                    model3.colorspace = ColorSpace.YCbCr;
        //                    model3.Opaque = true;
        //                    model = model3;
        //                    this.image = new Image(model, raster);
        //                }
        //                if (func2 == null)
        //                {
        //                    func2 = delegate(double x)
        //                    {
        //                        return (this.Units == UnitType.Inches) ? x : (x / 2.54);
        //                    };
        //                }
        //                Func<double, double> func = func2;
        //                this.image.DensityX = func((double)this.XDensity);
        //                this.image.DensityY = func((double)this.YDensity);
        //                this.height = frame.Height;
        //                this.width = frame.Width;
        //                goto Label_0998;
        //            }
        //        case 0xda:
        //            {
        //                Debug.WriteLine("Start of Scan (SOS)");
        //                ushort num17 = this.jpegReader.ReadShort();
        //                byte numberOfComponents = this.jpegReader.ReadByte();
        //                byte[] componentSelector = new byte[numberOfComponents];
        //                num2 = 0;
        //                while (num2 < numberOfComponents)
        //                {
        //                    byte num19 = this.jpegReader.ReadByte();
        //                    byte num20 = this.jpegReader.ReadByte();
        //                    int num21 = (num20 >> 4) & 15;
        //                    int num22 = num20 & 15;
        //                    frame.setHuffmanTables(num19, this.acTables[(byte)num22], this.dcTables[(byte)num21]);
        //                    componentSelector[num2] = num19;
        //                    num2++;
        //                }
        //                byte startSpectralSelection = this.jpegReader.ReadByte();
        //                byte endSpectralSelection = this.jpegReader.ReadByte();
        //                byte successiveApproximation = this.jpegReader.ReadByte();
        //                if (!this.progressive)
        //                {
        //                    frame.DecodeScanBaseline(numberOfComponents, componentSelector, resetInterval, this.jpegReader, ref this.marker);
        //                    flag = true;
        //                }
        //                if (this.progressive)
        //                {
        //                    frame.DecodeScanProgressive(successiveApproximation, startSpectralSelection, endSpectralSelection, numberOfComponents, componentSelector, resetInterval, this.jpegReader, ref this.marker);
        //                    flag = true;
        //                }
        //                goto Label_0998;
        //            }
        //        case 0xdb:
        //            {
        //                short num14 = (short)(this.jpegReader.ReadShort() - 2);
        //                for (int i = 0; i < (num14 / 0x41); i++)
        //                {
        //                    byte num16 = this.jpegReader.ReadByte();
        //                    int[] table = new int[0x40];
        //                    if (((byte)(num16 >> 4)) == 0)
        //                    {
        //                        for (num2 = 0; num2 < 0x40; num2++)
        //                        {
        //                            table[num2] = this.jpegReader.ReadByte();
        //                        }
        //                    }
        //                    else if (((byte)(num16 >> 4)) == 1)
        //                    {
        //                        for (num2 = 0; num2 < 0x40; num2++)
        //                        {
        //                            table[num2] = this.jpegReader.ReadShort();
        //                        }
        //                    }
        //                    this.qTables[num16 & 15] = new JpegQuantizationTable(table);
        //                }
        //                goto Label_0998;
        //            }
        //        case 220:
        //            frame.ScanLines = this.jpegReader.ReadShort();
        //            goto Label_0998;

        //        case 0xdd:
        //            this.jpegReader.BaseStream.Seek(2L, SeekOrigin.Current);
        //            resetInterval = this.jpegReader.ReadShort();
        //            goto Label_0998;

        //        case 0xe0:
        //        case 0xe1:
        //        case 0xe2:
        //        case 0xe3:
        //        case 0xe4:
        //        case 0xe5:
        //        case 230:
        //        case 0xe7:
        //        case 0xe8:
        //        case 0xe9:
        //        case 0xea:
        //        case 0xeb:
        //        case 0xec:
        //        case 0xed:
        //        case 0xee:
        //        case 0xef:
        //        case 0xfe:
        //            {
        //                JpegHeader item = this.ExtractHeader();
        //                if ((item.Marker == 0xe1) && (item.Data.Length >= 6))
        //                {
        //                    byte[] data = item.Data;
        //                    if (((((data[0] == 0x45) && (data[1] == 120)) && ((data[2] == 0x69) && (data[3] == 0x66))) && (data[4] == 0)) && (data[5] == 0))
        //                    {
        //                    }
        //                }
        //                if (((item.Data.Length >= 5) && (item.Marker == 0xee)) && (Encoding.UTF8.GetString(item.Data, 0, 5) == "Adobe"))
        //                {
        //                }
        //                metaHeaders.Add(item);
        //                if ((!flag2 && (this.marker == 0xe0)) && this.TryParseJFIF(item.Data))
        //                {
        //                    item.IsJFIF = true;
        //                    this.marker = this.jpegReader.GetNextMarker();
        //                    if (this.marker == 0xe0)
        //                    {
        //                        item = this.ExtractHeader();
        //                        metaHeaders.Add(item);
        //                    }
        //                    else
        //                    {
        //                        flag = true;
        //                    }
        //                }
        //                goto Label_0998;
        //            }
        //    }
        //    goto Label_0998;
        //}

        private JpegHeader ExtractHeader()
        {
            int count = this.jpegReader.ReadShort() - 2;
            byte[] buffer = new byte[count];
            this.jpegReader.Read(buffer, 0, count);
            JpegHeader header2 = new JpegHeader();
            header2.Marker = this.marker;
            header2.Data = buffer;
            return header2;
        }

        private bool TryParseJFIF(byte[] data)
        {
            FluxJpeg.Core.IO.BinaryReader reader = new FluxJpeg.Core.IO.BinaryReader(new MemoryStream(data));
            int num = data.Length + 2;
            if (num < JFIF_FIXED_LENGTH)
            {
                return false;
            }
            byte[] buffer = new byte[5];
            reader.Read(buffer, 0, buffer.Length);
            if ((((buffer[0] != 0x4a) || (buffer[1] != 70)) || ((buffer[2] != 0x49) || (buffer[3] != 70))) || (buffer[4] != 0))
            {
                return false;
            }
            this.majorVersion = reader.ReadByte();
            this.minorVersion = reader.ReadByte();
            if ((this.majorVersion != 1) || ((this.majorVersion == 1) && (this.minorVersion > 2)))
            {
                return false;
            }
            this.Units = (UnitType) reader.ReadByte();
            if (((this.Units != UnitType.None) && (this.Units != UnitType.Inches)) && (this.Units != UnitType.Centimeters))
            {
                return false;
            }
            this.XDensity = reader.ReadShort();
            this.YDensity = reader.ReadShort();
            this.Xthumbnail = reader.ReadByte();
            this.Ythumbnail = reader.ReadByte();
            int count = (3 * this.Xthumbnail) * this.Ythumbnail;
            if ((num > JFIF_FIXED_LENGTH) && (count != (num - JFIF_FIXED_LENGTH)))
            {
                return false;
            }
            if (count > 0)
            {
                this.thumbnail = new byte[count];
                if (reader.Read(this.thumbnail, 0, count) != count)
                {
                    return false;
                }
            }
            return true;
        }

        private void UpdateProgress(int stepsFinished, int stepsTotal)
        {
            if (this.DecodeProgressChanged != null)
            {
                this.DecodeProgress.DecodeProgress = ((double) stepsFinished) / ((double) stepsTotal);
                this.DecodeProgressChanged(this, this.DecodeProgress);
            }
        }

        private void UpdateStreamProgress(long StreamPosition)
        {
            if (this.DecodeProgressChanged != null)
            {
                this.DecodeProgress.ReadPosition = StreamPosition;
                this.DecodeProgressChanged(this, this.DecodeProgress);
            }
        }

        //public FluxJpeg.Core.Decoder.BlockUpsamplingMode BlockUpsamplingMode
        //{
        //    [CompilerGenerated]
        //    get
        //    {
        //        return this.<BlockUpsamplingMode>k__BackingField;
        //    }
        //    [CompilerGenerated]
        //    set
        //    {
        //        this.<BlockUpsamplingMode>k__BackingField = value;
        //    }
        //}

        private enum UnitType
        {
            None,
            Inches,
            Centimeters
        }
    }
}

