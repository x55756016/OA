namespace FluxJpeg.Core
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;

    public class DecodedJpeg
    {
        private FluxJpeg.Core.Image _image;
        private List<JpegHeader> _metaHeaders;
        [CompilerGenerated]
    //    private bool <HasJFIF>k__BackingField;
        internal int[] BlockHeight;
        internal int[] BlockWidth;
        internal int[] compHeight;
        internal int[] compWidth;
        internal int[] HsampFactor;
        internal bool[] lastColumnIsDummy;
        internal bool[] lastRowIsDummy;
        internal int MaxHsampFactor;
        internal int MaxVsampFactor;
        internal int Precision;
        internal int[] VsampFactor;

        public DecodedJpeg(FluxJpeg.Core.Image image) : this(image, null)
        {
            this._metaHeaders = new List<JpegHeader>();
            string s = "Jpeg Codec | fluxcapacity.net ";
            JpegHeader item = new JpegHeader();
            item.Marker = 0xfe;
            item.Data = Encoding.UTF8.GetBytes(s);
            this._metaHeaders.Add(item);
        }

        public DecodedJpeg(FluxJpeg.Core.Image image, IEnumerable<JpegHeader> metaHeaders)
        {
            this.Precision = 8;
            this.HsampFactor = new int[] { 1, 1, 1 };
            this.VsampFactor = new int[] { 1, 1, 1 };
            this.lastColumnIsDummy = new bool[3];
            this.lastRowIsDummy = new bool[3];
            this._image = image;
            this._metaHeaders = (metaHeaders == null) ? new List<JpegHeader>(0) : new List<JpegHeader>(metaHeaders);
            foreach (JpegHeader header in this._metaHeaders)
            {
                if (header.IsJFIF)
                {
                  //  this.HasJFIF = true;
                    break;
                }
            }
            int componentCount = this._image.ComponentCount;
            this.compWidth = new int[componentCount];
            this.compHeight = new int[componentCount];
            this.BlockWidth = new int[componentCount];
            this.BlockHeight = new int[componentCount];
            this.Initialize();
        }

        private void Initialize()
        {
            int num3;
            int width = this._image.Width;
            int height = this._image.Height;
            this.MaxHsampFactor = 1;
            this.MaxVsampFactor = 1;
            for (num3 = 0; num3 < this._image.ComponentCount; num3++)
            {
                this.MaxHsampFactor = Math.Max(this.MaxHsampFactor, this.HsampFactor[num3]);
                this.MaxVsampFactor = Math.Max(this.MaxVsampFactor, this.VsampFactor[num3]);
            }
            for (num3 = 0; num3 < this._image.ComponentCount; num3++)
            {
                this.compWidth[num3] = ((((width % 8) != 0) ? (((int) Math.Ceiling(((double) width) / 8.0)) * 8) : width) / this.MaxHsampFactor) * this.HsampFactor[num3];
                if (this.compWidth[num3] != ((width / this.MaxHsampFactor) * this.HsampFactor[num3]))
                {
                    this.lastColumnIsDummy[num3] = true;
                }
                this.BlockWidth[num3] = (int) Math.Ceiling(((double) this.compWidth[num3]) / 8.0);
                this.compHeight[num3] = ((((height % 8) != 0) ? (((int) Math.Ceiling(((double) height) / 8.0)) * 8) : height) / this.MaxVsampFactor) * this.VsampFactor[num3];
                if (this.compHeight[num3] != ((height / this.MaxVsampFactor) * this.VsampFactor[num3]))
                {
                    this.lastRowIsDummy[num3] = true;
                }
                this.BlockHeight[num3] = (int) Math.Ceiling(((double) this.compHeight[num3]) / 8.0);
            }
        }

        //public bool HasJFIF
        //{
        //    [CompilerGenerated]
        //    get
        //    {
        //        return this.<HasJFIF>k__BackingField;
        //    }
        //    [CompilerGenerated]
        //    private set
        //    {
        //        this.<HasJFIF>k__BackingField = value;
        //    }
        //}

        public FluxJpeg.Core.Image Image
        {
            get
            {
                return this._image;
            }
        }

        public IList<JpegHeader> MetaHeaders
        {
            get
            {
                return this._metaHeaders.AsReadOnly();
            }
        }
    }
}

