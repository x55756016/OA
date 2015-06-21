namespace FluxJpeg.Core
{
    using System;

    internal sealed class JPEGMarker
    {
        public const byte APP0 = 0xe0;
        public const byte APP1 = 0xe1;
        public const byte APP10 = 0xea;
        public const byte APP11 = 0xeb;
        public const byte APP12 = 0xec;
        public const byte APP13 = 0xed;
        public const byte APP14 = 0xee;
        public const byte APP15 = 0xef;
        public const byte APP2 = 0xe2;
        public const byte APP3 = 0xe3;
        public const byte APP4 = 0xe4;
        public const byte APP5 = 0xe5;
        public const byte APP6 = 230;
        public const byte APP7 = 0xe7;
        public const byte APP8 = 0xe8;
        public const byte APP9 = 0xe9;
        public const byte COM = 0xfe;
        public const byte DHT = 0xc4;
        public const byte DNL = 220;
        public const byte DQT = 0xdb;
        public const byte DRI = 0xdd;
        public const byte EOI = 0xd9;
        public const byte JFIF_F = 70;
        public const byte JFIF_I = 0x49;
        public const byte JFIF_J = 0x4a;
        public const byte JFIF_X = 70;
        public const byte JFXX_JPEG = 0x10;
        public const byte JFXX_ONE_BPP = 0x11;
        public const byte JFXX_THREE_BPP = 0x13;
        public const byte RST0 = 0xd0;
        public const byte RST1 = 0xd1;
        public const byte RST2 = 210;
        public const byte RST3 = 0xd3;
        public const byte RST4 = 0xd4;
        public const byte RST5 = 0xd5;
        public const byte RST6 = 0xd6;
        public const byte RST7 = 0xd7;
        public const byte SOF0 = 0xc0;
        public const byte SOF1 = 0xc1;
        public const byte SOF10 = 0xca;
        public const byte SOF11 = 0xcb;
        public const byte SOF13 = 0xcd;
        public const byte SOF14 = 0xce;
        public const byte SOF15 = 0xcf;
        public const byte SOF2 = 0xc2;
        public const byte SOF3 = 0xc3;
        public const byte SOF5 = 0xc5;
        public const byte SOF6 = 0xc6;
        public const byte SOF7 = 0xc7;
        public const byte SOF9 = 0xc9;
        public const byte SOI = 0xd8;
        public const byte SOS = 0xda;
        public const byte X00 = 0;
        public const byte XFF = 0xff;
    }
}

