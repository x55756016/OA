namespace FluxJpeg.Core
{
    using System;
    using System.Text;

    public class JpegHeader
    {
        public byte[] Data;
        internal bool IsJFIF = false;
        public byte Marker;

        public string ToString
        {
            get
            {
                return Encoding.UTF8.GetString(this.Data, 0, this.Data.Length);
            }
        }
    }
}

