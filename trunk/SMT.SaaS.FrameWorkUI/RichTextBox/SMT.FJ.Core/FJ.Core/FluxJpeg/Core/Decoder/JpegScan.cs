namespace FluxJpeg.Core.Decoder
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class JpegScan
    {
        private List<JpegComponent> components = new List<JpegComponent>();
        private int maxH = 0;
        private int maxV = 0;

        public void AddComponent(byte id, byte factorHorizontal, byte factorVertical, byte quantizationID, byte colorMode)
        {
            JpegComponent item = new JpegComponent(this, id, factorHorizontal, factorVertical, quantizationID, colorMode);
            this.components.Add(item);
            this.maxH = this.components.Max<JpegComponent, byte>(delegate (JpegComponent x) {
                return x.factorH;
            });
            this.maxV = this.components.Max<JpegComponent, byte>(delegate (JpegComponent x) {
                return x.factorV;
            });
        }

        public JpegComponent GetComponentById(byte Id)
        {
            return this.components.First<JpegComponent>(delegate (JpegComponent x) {
                return (x.component_id == Id);
            });
        }

        public IList<JpegComponent> Components
        {
            get
            {
                return this.components.AsReadOnly();
            }
        }

        internal int MaxH
        {
            get
            {
                return this.maxH;
            }
        }

        internal int MaxV
        {
            get
            {
                return this.maxV;
            }
        }
    }
}

