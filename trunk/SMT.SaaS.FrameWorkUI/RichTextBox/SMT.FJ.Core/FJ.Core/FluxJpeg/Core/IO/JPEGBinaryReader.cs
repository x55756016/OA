namespace FluxJpeg.Core.IO
{
    using System;
    using System.IO;

    internal class JPEGBinaryReader : FluxJpeg.Core.IO.BinaryReader
    {
        private byte _bitBuffer;
        protected int _bitsLeft;
        public int eob_run;
        private byte marker;

        public JPEGBinaryReader(Stream input) : base(input)
        {
            this.eob_run = 0;
            this._bitsLeft = 0;
        }

        public byte GetNextMarker()
        {
            byte marker;
            try
            {
                bool flag;
                goto Label_000D;
            Label_0004:
                this.ReadJpegByte();
            Label_000D:
                flag = true;
                goto Label_0004;
            }
            catch (JPEGMarkerFoundException exception)
            {
                marker = exception.Marker;
            }
            return marker;
        }

        public int ReadBits(int n)
        {
            int num = 0;
            if (this._bitsLeft >= n)
            {
                this._bitsLeft -= n;
                num = this._bitBuffer >> (8 - n);
                this._bitBuffer = (byte) (this._bitBuffer << n);
                return num;
            }
            while (n > 0)
            {
                if (this._bitsLeft == 0)
                {
                    this._bitBuffer = this.ReadJpegByte();
                    this._bitsLeft = 8;
                }
                int num2 = (n <= this._bitsLeft) ? n : this._bitsLeft;
                num |= (this._bitBuffer >> ((8 - num2) & 0x1f)) << (n - num2);
                this._bitBuffer = (byte) (this._bitBuffer << num2);
                this._bitsLeft -= num2;
                n -= num2;
            }
            return num;
        }

        protected byte ReadJpegByte()
        {
            byte num = base.ReadByte();
            if (num != 0xff)
            {
                return num;
            }
            while ((num = base.ReadByte()) == 0xff)
            {
            }
            if (num == 0)
            {
                return 0xff;
            }
            this.marker = num;
            throw new JPEGMarkerFoundException(this.marker);
        }

        public int BitOffset
        {
            get
            {
                return ((8 - this._bitsLeft) % 8);
            }
            set
            {
                if (this._bitsLeft != 0)
                {
                    base.BaseStream.Seek(-1L, SeekOrigin.Current);
                }
                this._bitsLeft = (8 - value) % 8;
            }
        }
    }
}

