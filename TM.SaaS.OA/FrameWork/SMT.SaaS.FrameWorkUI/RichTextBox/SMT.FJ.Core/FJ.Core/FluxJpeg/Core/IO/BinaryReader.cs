namespace FluxJpeg.Core.IO
{
    using System;
    using System.IO;

    internal class BinaryReader
    {
        private byte[] _buffer;
        private Stream _stream;

        public BinaryReader(byte[] data) : this(new MemoryStream(data))
        {
        }

        public BinaryReader(Stream stream)
        {
            this._stream = stream;
            this._buffer = new byte[2];
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return this._stream.Read(buffer, offset, count);
        }

        public byte ReadByte()
        {
            int num = this._stream.ReadByte();
            if (num == -1)
            {
                throw new EndOfStreamException();
            }
            return (byte) num;
        }

        public ushort ReadShort()
        {
            this._stream.Read(this._buffer, 0, 2);
            return (ushort) ((this._buffer[0] << 8) | (this._buffer[1] & 0xff));
        }

        public Stream BaseStream
        {
            get
            {
                return this._stream;
            }
        }
    }
}

