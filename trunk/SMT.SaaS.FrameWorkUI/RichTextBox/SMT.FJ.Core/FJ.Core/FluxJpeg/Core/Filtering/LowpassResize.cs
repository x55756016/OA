namespace FluxJpeg.Core.Filtering
{
    using System;

    internal class LowpassResize : FluxJpeg.Core.Filtering.Filter
    {
        protected override void ApplyFilter()
        {
            int length = base._sourceData[0].GetLength(0);
            int num2 = base._sourceData[0].GetLength(1);
            int num3 = base._sourceData.Length;
            double std = (length / base._newWidth) * 0.5;
            for (int i = 0; i < num3; i++)
            {
                GrayImage data = new GrayImage(base._sourceData[i]);
                base._sourceData[i] = Convolution.Instance.GaussianConv(data, std).ToByteArray2D();
            }
            double num6 = ((double) length) / ((double) base._newWidth);
            double num7 = ((double) num2) / ((double) base._newHeight);
            base._destinationData = new NNResize().Apply(base._sourceData, base._newWidth, base._newHeight);
        }
    }
}

