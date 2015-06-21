namespace FluxJpeg.Core.Filtering
{
    using System;

    internal class NNResize : FluxJpeg.Core.Filtering.Filter
    {
        protected override void ApplyFilter()
        {
            int length = base._sourceData[0].GetLength(0);
            int num2 = base._sourceData[0].GetLength(1);
            double num3 = ((double) length) / ((double) base._newWidth);
            double num4 = ((double) num2) / ((double) base._newHeight);
            double num5 = 0.5 * num3;
            double num6 = 0.5 * num4;
            for (int i = 0; i < base._newHeight; i++)
            {
                int num7 = (int) num6;
                num5 = 0.0;
                base.UpdateProgress(((double) i) / ((double) base._newHeight));
                for (int j = 0; j < base._newWidth; j++)
                {
                    int num8 = (int) num5;
                    base._destinationData[0][j, i] = base._sourceData[0][num8, num7];
                    if (base._color)
                    {
                        base._destinationData[1][j, i] = base._sourceData[1][num8, num7];
                        base._destinationData[2][j, i] = base._sourceData[2][num8, num7];
                    }
                    num5 += num3;
                }
                num6 += num4;
            }
        }
    }
}

