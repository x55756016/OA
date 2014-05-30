namespace FluxJpeg.Core.Filtering
{
    using System;
    using System.Reflection;

    public class GrayImage
    {
        private int _height;
        private int _width;
        public float[] Scan0;

        public GrayImage(byte[,] channel)
        {
            this.Convert(channel);
        }

        public GrayImage(int width, int height)
        {
            this._width = width;
            this._height = height;
            this.Scan0 = new float[width * height];
        }

        private void Convert(byte[,] channel)
        {
            this._width = channel.GetLength(0);
            this._height = channel.GetLength(1);
            this.Scan0 = new float[this._width * this._height];
            int num = 0;
            for (int i = 0; i < this._height; i++)
            {
                for (int j = 0; j < this._width; j++)
                {
                    this.Scan0[num++] = ((float) channel[j, i]) / 255f;
                }
            }
        }

        public byte[,] ToByteArray2D()
        {
            byte[,] buffer = new byte[this._width, this._height];
            int num = 0;
            for (int i = 0; i < this._height; i++)
            {
                for (int j = 0; j < this._width; j++)
                {
                    buffer[j, i] = (byte) (this.Scan0[num++] * 255f);
                }
            }
            return buffer;
        }

        public int Height
        {
            get
            {
                return this._height;
            }
        }

        public float this[int x, int y]
        {
            get
            {
                return this.Scan0[(y * this._width) + x];
            }
            set
            {
                this.Scan0[(y * this._width) + x] = value;
            }
        }

        public int Width
        {
            get
            {
                return this._width;
            }
        }
    }
}

