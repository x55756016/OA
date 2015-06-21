namespace FluxJpeg.Core.Decoder
{
    using System;

    public class JpegDecodeProgressChangedArgs : EventArgs
    {
        public bool Abort;
        public double DecodeProgress;
        public int Height;
        public long ReadPosition;
        public bool SizeReady;
        public int Width;
    }
}

