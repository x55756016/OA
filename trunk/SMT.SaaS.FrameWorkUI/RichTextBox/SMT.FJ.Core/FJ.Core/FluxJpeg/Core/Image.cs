namespace FluxJpeg.Core
{
    using System;
    using System.Runtime.CompilerServices;

    public class Image
    {
        private FluxJpeg.Core.ColorModel _cm;
        private byte[][,] _raster;
        //[CompilerGenerated]
        //private double <DensityX>k__BackingField;
        //[CompilerGenerated]
        //private double <DensityY>k__BackingField;
        private int height;
        private int width;

        public Image(FluxJpeg.Core.ColorModel cm, byte[][,] raster)
        {
            this.width = raster[0].GetLength(0);
            this.height = raster[0].GetLength(1);
            this._cm = cm;
            this._raster = raster;
        }

        public Image ChangeColorSpace(ColorSpace cs)
        {
            if (this._cm.colorspace != cs)
            {
                int num;
                int num2;
                byte[] buffer = new byte[3];
                byte[] buffer2 = new byte[3];
                if ((this._cm.colorspace == ColorSpace.RGB) && (cs == ColorSpace.YCbCr))
                {
                    for (num = 0; num < this.width; num++)
                    {
                        num2 = 0;
                        while (num2 < this.height)
                        {
                            YCbCr.fromRGB(ref this._raster[0][num, num2], ref this._raster[1][num, num2], ref this._raster[2][num, num2]);
                            num2++;
                        }
                    }
                    this._cm.colorspace = ColorSpace.YCbCr;
                }
                else if ((this._cm.colorspace == ColorSpace.YCbCr) && (cs == ColorSpace.RGB))
                {
                    for (num = 0; num < this.width; num++)
                    {
                        num2 = 0;
                        while (num2 < this.height)
                        {
                            YCbCr.toRGB(ref this._raster[0][num, num2], ref this._raster[1][num, num2], ref this._raster[2][num, num2]);
                            num2++;
                        }
                    }
                    this._cm.colorspace = ColorSpace.RGB;
                }
                else if ((this._cm.colorspace == ColorSpace.Gray) && (cs == ColorSpace.YCbCr))
                {
                    byte[,] buffer3 = new byte[this.width, this.height];
                    byte[,] buffer4 = new byte[this.width, this.height];
                    for (num = 0; num < this.width; num++)
                    {
                        for (num2 = 0; num2 < this.height; num2++)
                        {
                            buffer3[num, num2] = 0x80;
                            buffer4[num, num2] = 0x80;
                        }
                    }
                    this._raster = new byte[][,] { this._raster[0], buffer3, buffer4 };
                    this._cm.colorspace = ColorSpace.YCbCr;
                }
                else
                {
                    if ((this._cm.colorspace != ColorSpace.Gray) || (cs != ColorSpace.RGB))
                    {
                        throw new Exception("Colorspace conversion not supported.");
                    }
                    this.ChangeColorSpace(ColorSpace.YCbCr);
                    this.ChangeColorSpace(ColorSpace.RGB);
                }
            }
            return this;
        }

        public static byte[][,] CreateRaster(int width, int height, int bands)
        {
            byte[][,] bufferArray = new byte[bands][,];
            for (int i = 0; i < bands; i++)
            {
                bufferArray[i] = new byte[width, height];
            }
            return bufferArray;
        }

        public FluxJpeg.Core.ColorModel ColorModel
        {
            get
            {
                return this._cm;
            }
        }

        public int ComponentCount
        {
            get
            {
                return this._raster.Length;
            }
        }

        //public double DensityX
        //{
        //    [CompilerGenerated]
        //    get
        //    {
        //        return this.<DensityX>k__BackingField;
        //    }
        //    [CompilerGenerated]
        //    set
        //    {
        //        this.<DensityX>k__BackingField = value;
        //    }
        //}

        //public double DensityY
        //{
        //    [CompilerGenerated]
        //    get
        //    {
        //        return this.<DensityY>k__BackingField;
        //    }
        //    [CompilerGenerated]
        //    set
        //    {
        //        this.<DensityY>k__BackingField = value;
        //    }
        //}

        public int Height
        {
            get
            {
                return this.height;
            }
        }

        public byte[][,] Raster
        {
            get
            {
                return this._raster;
            }
        }

        public int Width
        {
            get
            {
                return this.width;
            }
        }

        private delegate void ConvertColor(ref byte c1, ref byte c2, ref byte c3);
    }
}

