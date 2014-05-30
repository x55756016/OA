namespace FluxJpeg.Core
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct ColorModel
    {
        public ColorSpace colorspace;
        public bool Opaque;
    }
}

