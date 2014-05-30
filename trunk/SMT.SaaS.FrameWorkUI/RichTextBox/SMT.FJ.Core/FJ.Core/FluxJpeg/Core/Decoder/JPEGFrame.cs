namespace FluxJpeg.Core.Decoder
{
    using FluxJpeg.Core;
    using FluxJpeg.Core.IO;
    using System;
    using System.Runtime.CompilerServices;

    internal class JPEGFrame
    {
        //[CompilerGenerated]
        //private byte <ComponentCount>k__BackingField;
        //[CompilerGenerated]
        //private ushort <Height>k__BackingField;
        //[CompilerGenerated]
        //private ushort <Width>k__BackingField;
        public byte colorMode = JPEG_COLOR_YCbCr;
        public static byte JPEG_COLOR_CMYK = 4;
        public static byte JPEG_COLOR_GRAY = 1;
        public static byte JPEG_COLOR_RGB = 2;
        public static byte JPEG_COLOR_YCbCr = 3;
        public byte precision = 8;
        public Action<long> ProgressUpdateMethod = null;
        public JpegScan Scan = new JpegScan();

        public void AddComponent(byte componentID, byte sampleHFactor, byte sampleVFactor, byte quantizationTableID)
        {
            this.Scan.AddComponent(componentID, sampleHFactor, sampleVFactor, quantizationTableID, this.colorMode);
        }

        //private void DecodeScan(byte numberOfComponents, byte[] componentSelector, int resetInterval, JPEGBinaryReader jpegReader, ref byte marker)
        //{
        //    jpegReader.eob_run = 0;
        //    int idx = 0;
        //    int num2 = 0;
        //    int i = 0;
        //    int j = 0;
        //    int num5 = 0;
        //    long position = jpegReader.BaseStream.Position;
        //    while (true)
        //    {
        //        JpegComponent componentById;
        //        int num9;
        //        if ((this.ProgressUpdateMethod != null) && (jpegReader.BaseStream.Position >= (position + JpegDecoder.ProgressUpdateByteInterval)))
        //        {
        //            position = jpegReader.BaseStream.Position;
        //            this.ProgressUpdateMethod(position);
        //        }
        //        try
        //        {
        //            if (numberOfComponents == 1)
        //            {
        //                componentById = this.Scan.GetComponentById(componentSelector[0]);
        //                componentById.SetBlock(idx);
        //                componentById.DecodeMCU(jpegReader, i, j);
        //                int num7 = this.mcus_per_row(componentById);
        //                int num8 = (int) Math.Ceiling(((double) this.Width) / ((double) (8 * componentById.factorH)));
        //                i++;
        //                num5++;
        //                if (i == componentById.factorH)
        //                {
        //                    i = 0;
        //                    idx++;
        //                }
        //                if ((num5 % num7) == 0)
        //                {
        //                    num5 = 0;
        //                    j++;
        //                    if (j == componentById.factorV)
        //                    {
        //                        if (i != 0)
        //                        {
        //                            idx++;
        //                            i = 0;
        //                        }
        //                        j = 0;
        //                    }
        //                    else
        //                    {
        //                        idx -= num8;
        //                        if (i != 0)
        //                        {
        //                            idx++;
        //                            i = 0;
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                num9 = 0;
        //                while (num9 < numberOfComponents)
        //                {
        //                    componentById = this.Scan.GetComponentById(componentSelector[num9]);
        //                    componentById.SetBlock(num2);
        //                    for (int k = 0; k < componentById.factorV; k++)
        //                    {
        //                        for (int m = 0; m < componentById.factorH; m++)
        //                        {
        //                            componentById.DecodeMCU(jpegReader, m, k);
        //                        }
        //                    }
        //                    num9++;
        //                }
        //                idx++;
        //                num2++;
        //            }
        //        }
        //        catch (JPEGMarkerFoundException exception)
        //        {
        //            marker = exception.Marker;
        //            if (((((marker != 0xd0) && (marker != 0xd1)) && ((marker != 210) && (marker != 0xd3))) && (((marker != 0xd4) && (marker != 0xd5)) && (marker != 0xd6))) && (marker != 0xd7))
        //            {
        //                return;
        //            }
        //            for (num9 = 0; num9 < numberOfComponents; num9++)
        //            {
        //                componentById = this.Scan.GetComponentById(componentSelector[num9]);
        //                if (num9 > 1)
        //                {
        //                    componentById.padMCU(num2, resetInterval - idx);
        //                }
        //                componentById.resetInterval();
        //            }
        //            num2 += resetInterval - idx;
        //            idx = 0;
        //        }
        //    }
        //}

        //public void DecodeScanBaseline(byte numberOfComponents, byte[] componentSelector, int resetInterval, JPEGBinaryReader jpegReader, ref byte marker)
        //{
        //    for (int i = 0; i < numberOfComponents; i++)
        //    {
        //        JpegComponent componentById = this.Scan.GetComponentById(componentSelector[i]);
        //        componentById.Decode = new JpegComponent.DecodeFunction(componentById.DecodeBaseline);
        //    }
        //    this.DecodeScan(numberOfComponents, componentSelector, resetInterval, jpegReader, ref marker);
        //}

        public void DecodeScanProgressive(byte successiveApproximation, byte startSpectralSelection, byte endSpectralSelection, byte numberOfComponents, byte[] componentSelector, int resetInterval, JPEGBinaryReader jpegReader, ref byte marker)
        {
            byte num = (byte) (successiveApproximation >> 4);
            byte num2 = (byte) (successiveApproximation & 15);
            if ((startSpectralSelection > endSpectralSelection) || (endSpectralSelection > 0x3f))
            {
                throw new Exception("Bad spectral selection.");
            }
            bool flag = startSpectralSelection == 0;
            bool flag2 = num != 0;
            if (flag)
            {
                if (endSpectralSelection != 0)
                {
                    throw new Exception("Bad spectral selection for DC only scan.");
                }
            }
            else if (numberOfComponents > 1)
            {
                throw new Exception("Too many components for AC scan!");
            }
            for (int i = 0; i < numberOfComponents; i++)
            {
                JpegComponent componentById = this.Scan.GetComponentById(componentSelector[i]);
                componentById.successiveLow = num2;
                if (flag)
                {
                    if (flag2)
                    {
                        componentById.Decode = new JpegComponent.DecodeFunction(componentById.DecodeDCRefine);
                    }
                    else
                    {
                        componentById.Decode = new JpegComponent.DecodeFunction(componentById.DecodeDCFirst);
                    }
                }
                else
                {
                    componentById.spectralStart = startSpectralSelection;
                    componentById.spectralEnd = endSpectralSelection;
                    if (flag2)
                    {
                        componentById.Decode = new JpegComponent.DecodeFunction(componentById.DecodeACRefine);
                    }
                    else
                    {
                        componentById.Decode = new JpegComponent.DecodeFunction(componentById.DecodeACFirst);
                    }
                }
            }
         //   this.DecodeScan(numberOfComponents, componentSelector, resetInterval, jpegReader, ref marker);
        }

        //private int mcus_per_row(JpegComponent c)
        //{
        //    return (((((this.Width * c.factorH) + (this.Scan.MaxH - 1)) / this.Scan.MaxH) + 7) / 8);
        //}

        public void setHuffmanTables(byte componentID, JpegHuffmanTable ACTable, JpegHuffmanTable DCTable)
        {
            JpegComponent componentById = this.Scan.GetComponentById(componentID);
            if (DCTable != null)
            {
                componentById.setDCTable(DCTable);
            }
            if (ACTable != null)
            {
                componentById.setACTable(ACTable);
            }
        }

        public void setPrecision(byte data)
        {
            this.precision = data;
        }

        //public byte ColorMode
        //{
        //    get
        //    {
        //        return ((this.ComponentCount == 1) ? JPEG_COLOR_GRAY : JPEG_COLOR_YCbCr);
        //    }
        //}

        //public byte ComponentCount
        //{
        //    [CompilerGenerated]
        //    get
        //    {
        //        return this.<ComponentCount>k__BackingField;
        //    }
        //    [CompilerGenerated]
        //    set
        //    {
        //        this.<ComponentCount>k__BackingField = value;
        //    }
        //}

        //public ushort Height
        //{
        //    [CompilerGenerated]
        //    get
        //    {
        //        return this.<Height>k__BackingField;
        //    }
        //    [CompilerGenerated]
        //    private set
        //    {
        //        this.<Height>k__BackingField = value;
        //    }
        //}

        //public ushort SamplesPerLine
        //{
        //    set
        //    {
        //        this.Width = value;
        //    }
        //}

        //public ushort ScanLines
        //{
        //    set
        //    {
        //        this.Height = value;
        //    }
        //}

        //public ushort Width
        //{
        //    [CompilerGenerated]
        //    get
        //    {
        //        return this.<Width>k__BackingField;
        //    }
        //    [CompilerGenerated]
        //    private set
        //    {
        //        this.<Width>k__BackingField = value;
        //    }
        //}
    }
}

