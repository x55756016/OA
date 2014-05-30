namespace FluxJpeg.Core
{
    using FluxJpeg.Core.Filtering;
    using System;
    using System.Runtime.CompilerServices;

    public class ImageResizer
    {
        private Image _input;
        private ResizeProgressChangedEventArgs progress = new ResizeProgressChangedEventArgs();

        public event EventHandler<ResizeProgressChangedEventArgs> ProgressChanged;

        public ImageResizer(Image input)
        {
            this._input = input;
        }

        public Image Resize(double scale, ResamplingFilters technique)
        {
            FluxJpeg.Core.Filtering.Filter filter;
            int newHeight = (int) (scale * this._input.Height);
            int newWidth = (int) (scale * this._input.Width);
            switch (technique)
            {
                case ResamplingFilters.NearestNeighbor:
                    filter = new NNResize();
                    break;

                case ResamplingFilters.LowpassAntiAlias:
                    filter = new LowpassResize();
                    break;

                default:
                    throw new NotSupportedException();
            }
            return new Image(this._input.ColorModel, filter.Apply(this._input.Raster, newWidth, newHeight));
        }

        public Image Resize(int maxEdgeLength, ResamplingFilters technique)
        {
            double scale = 0.0;
            if (this._input.Width > this._input.Height)
            {
                scale = ((double) maxEdgeLength) / ((double) this._input.Width);
            }
            else
            {
                scale = ((double) maxEdgeLength) / ((double) this._input.Height);
            }
            if (scale >= 1.0)
            {
                throw new ResizeNotNeededException();
            }
            return this.Resize(scale, technique);
        }

        public Image Resize(int maxWidth, int maxHeight, ResamplingFilters technique)
        {
            double num = ((double) maxWidth) / ((double) this._input.Width);
            double num2 = ((double) maxHeight) / ((double) this._input.Height);
            double scale = 0.0;
            if (num < num2)
            {
                scale = num;
            }
            else
            {
                scale = num2;
            }
            if (scale >= 1.0)
            {
                throw new ResizeNotNeededException();
            }
            return this.Resize(scale, technique);
        }

        public static bool ResizeNeeded(Image image, int maxEdgeLength)
        {
            double num = (image.Width > image.Height) ? (((double) maxEdgeLength) / ((double) image.Width)) : (((double) maxEdgeLength) / ((double) image.Height));
            return (num < 1.0);
        }

        private void ResizeProgressChanged(object sender, FilterProgressEventArgs e)
        {
            this.progress.Progress = e.Progress;
            if (this.ProgressChanged != null)
            {
                this.ProgressChanged(this, this.progress);
            }
        }
    }
}

