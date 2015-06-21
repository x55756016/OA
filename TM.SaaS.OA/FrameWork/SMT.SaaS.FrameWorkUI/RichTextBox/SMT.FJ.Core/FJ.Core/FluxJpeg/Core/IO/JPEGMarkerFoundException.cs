namespace FluxJpeg.Core.IO
{
    using System;

    internal class JPEGMarkerFoundException : Exception
    {
        public byte Marker;

        public JPEGMarkerFoundException(byte marker)
        {
            this.Marker = marker;
        }
    }
}

