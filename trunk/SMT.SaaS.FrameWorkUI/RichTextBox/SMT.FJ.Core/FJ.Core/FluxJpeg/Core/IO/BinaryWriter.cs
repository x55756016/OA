namespace FluxJpeg.Core.IO
{
    using System;
    using System.IO;

    internal class BinaryWriter
    {
        private Stream _stream;

        internal BinaryWriter(Stream stream)
        {
            this._stream = stream;
        }

        internal void Write(byte[] val)
        {
            this._stream.Write(val, 0, val.Length);
        }

        internal void Write(byte val)
        {
            this._stream.WriteByte(val);
        }

        internal void Write(short val)
        {
            this._stream.WriteByte((byte) ((val >> 8) & 0xff));
            this._stream.WriteByte((byte) (val & 0xff));
        }

        internal void Write(byte[] val, int offset, int count)
        {
            this._stream.Write(val, offset, count);
        }
    }
}

