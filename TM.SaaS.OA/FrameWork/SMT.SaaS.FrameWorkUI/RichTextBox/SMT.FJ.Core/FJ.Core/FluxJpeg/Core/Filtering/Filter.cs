namespace FluxJpeg.Core.Filtering
{
    using FluxJpeg.Core;
    using System;
    using System.Runtime.CompilerServices;

    internal abstract class Filter
    {
        protected bool _color;
        protected byte[][,] _destinationData;
        protected int _newHeight;
        protected int _newWidth;
        protected byte[][,] _sourceData;
        private FilterProgressEventArgs progressArgs = new FilterProgressEventArgs();

        public event EventHandler<FilterProgressEventArgs> ProgressChanged;

        protected Filter()
        {
        }

        public byte[][,] Apply(byte[][,] imageData, int newWidth, int newHeight)
        {
            this._newHeight = newHeight;
            this._newWidth = newWidth;
            this._color = imageData.Length != 1;
            this._destinationData = Image.CreateRaster(newWidth, newHeight, imageData.Length);
            this._sourceData = imageData;
            this.ApplyFilter();
            return this._destinationData;
        }

        protected abstract void ApplyFilter();
        protected void UpdateProgress(double progress)
        {
            this.progressArgs.Progress = progress;
            if (this.ProgressChanged != null)
            {
                this.ProgressChanged(this, this.progressArgs);
            }
        }
    }
}

