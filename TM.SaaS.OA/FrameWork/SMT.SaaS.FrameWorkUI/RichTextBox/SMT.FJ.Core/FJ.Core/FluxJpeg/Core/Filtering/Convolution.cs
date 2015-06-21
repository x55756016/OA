namespace FluxJpeg.Core.Filtering
{
    using System;
    using System.Runtime.InteropServices;
    using System.Threading;

    public class Convolution
    {
        public static readonly Convolution Instance = new Convolution();

        public GrayImage Conv2D(GrayImage data, GrayImage op)
        {
            GrayImage image = new GrayImage(data.Width, data.Height);
            if (((op.Width % 2) == 0) || ((op.Height % 2) == 0))
            {
                throw new ArgumentException("Operator must have an odd number of rows and columns.");
            }
            int num = op.Width / 2;
            int num2 = op.Height / 2;
            for (int i = 0; i < data.Height; i++)
            {
                for (int j = 0; j < data.Width; j++)
                {
                    float num5 = 0f;
                    float num6 = 0f;
                    for (int k = 0; k < op.Height; k++)
                    {
                        int num8 = (i - num2) + k;
                        if ((num8 >= 0) && (num8 < data.Height))
                        {
                            for (int m = 0; m < op.Width; m++)
                            {
                                int num10 = (j - num) + m;
                                if ((num10 >= 0) && (num10 < data.Width))
                                {
                                    float num11 = op[m, k];
                                    num6 += Math.Abs(num11);
                                    num5 += data[num10, num8] * num11;
                                }
                            }
                        }
                    }
                    image[j, i] = num5 / num6;
                }
            }
            return image;
        }

        public GrayImage Conv2DSeparable(GrayImage data, float[] filter)
        {
            GrayImage image = this.Filter1DSymmetric(data, filter, true);
            return this.Filter1DSymmetric(image, filter, true);
        }

        private GrayImage Conv2DSymm(GrayImage data, GrayImage opLR)
        {
            if (((opLR.Width % 2) != 0) || ((opLR.Height % 2) != 0))
            {
                throw new ArgumentException("Operator must have an even number of rows and columns.");
            }
            int num = opLR.Width - 1;
            int num2 = opLR.Height - 1;
            GrayImage image = new GrayImage(data.Width - (2 * num), data.Height - (2 * num2));
            for (int i = num2; i < (data.Height - num2); i++)
            {
                for (int j = num; j < (data.Width - num); j++)
                {
                    float num5 = data[j, i] * opLR.Scan0[0];
                    int num6 = 1;
                    while (num6 < opLR.Height)
                    {
                        num5 += (data[j, i + num6] + data[j, i - num6]) * opLR[0, num6];
                        num6++;
                    }
                    int num7 = 1;
                    while (num7 < opLR.Width)
                    {
                        num5 += (data[j + num7, i] + data[j - num7, i]) * opLR[num7, 0];
                        num7++;
                    }
                    int index = 1;
                    for (num6 = 1; num6 < opLR.Height; num6++)
                    {
                        int num9 = ((i + num6) * data.Width) + j;
                        int num10 = ((i - num6) * data.Width) + j;
                        for (num7 = 1; num7 < opLR.Width; num7++)
                        {
                            num5 += (((data.Scan0[num9 + num7] + data.Scan0[num10 + num7]) + data.Scan0[num9 - num7]) + data.Scan0[num10 - num7]) * opLR.Scan0[index];
                            index++;
                        }
                        index++;
                    }
                    image[j - num, i - num2] = num5;
                }
            }
            return image;
        }

        public GrayImage Conv2DSymmetric(GrayImage data, GrayImage opLR)
        {
            int num = opLR.Width - 1;
            int num2 = opLR.Height - 1;
            GrayImage image = new GrayImage(data.Width + (2 * num), data.Height + (2 * num2));
            int index = 0;
            for (int i = 0; i < data.Height; i++)
            {
                int num5 = ((i + num2) * (data.Width + (2 * num))) + num;
                for (int j = 0; j < data.Width; j++)
                {
                    image.Scan0[num5 + j] = data.Scan0[index];
                    index++;
                }
            }
            return this.Conv2DSymm(image, opLR);
        }

        public GrayImage Filter1DSymmetric(GrayImage data, float[] filter, bool transpose)
        {
            GrayImage image = transpose ? new GrayImage(data.Height, data.Width) : new GrayImage(data.Width, data.Height);
            int num = 0;
            int num2 = transpose ? num : (num * image.Width);
            FilterJob job2 = new FilterJob();
            job2.filter = filter;
            job2.data = data;
            job2.destPtr = num2;
            job2.result = image;
            job2.start = num;
            job2.end = data.Height / 2;
            FilterJob parameter = job2;
            ParameterizedThreadStart start = transpose ? new ParameterizedThreadStart(this.FilterPartSymmetricT) : new ParameterizedThreadStart(this.FilterPartSymmetric);
            Thread thread = new Thread(start);
            thread.Start(parameter);
            num = data.Height / 2;
            num2 = transpose ? num : (num * image.Width);
            parameter.start = num;
            parameter.destPtr = num2;
            parameter.end = data.Height;
            start(parameter);
            thread.Join();
            return image;
        }

        private void FilterPartSymmetric(object filterJob)
        {
            FilterJob job = (FilterJob) filterJob;
            GrayImage data = job.data;
            float[] numArray = data.Scan0;
            float[] filter = job.filter;
            float[] numArray3 = job.result.Scan0;
            int num = filter.Length - 1;
            int destPtr = job.destPtr;
            for (int i = job.start; i < job.end; i++)
            {
                float num7;
                int num8;
                int num4 = i * data.Width;
                int index = job.dataPtr + num4;
                int num6 = 0;
                while (num6 < num)
                {
                    num7 = numArray[index] * filter[0];
                    num8 = 1;
                    while (num8 < (num6 + 1))
                    {
                        num7 += (numArray[index + num8] + numArray[index - num8]) * filter[num8];
                        num8++;
                    }
                    num8 = num6 + 1;
                    while (num8 < filter.Length)
                    {
                        num7 += (numArray[index + num8] + numArray[index + num8]) * filter[num8];
                        num8++;
                    }
                    numArray3[destPtr++] = num7;
                    index++;
                    num6++;
                }
                num6 = num;
                while (num6 < (data.Width - num))
                {
                    num7 = numArray[index] * filter[0];
                    num8 = 1;
                    while (num8 < filter.Length)
                    {
                        num7 += (numArray[index + num8] + numArray[index - num8]) * filter[num8];
                        num8++;
                    }
                    numArray3[destPtr++] = num7;
                    index++;
                    num6++;
                }
                for (num6 = data.Width - num; num6 < data.Width; num6++)
                {
                    num7 = numArray[index] * filter[0];
                    num8 = 0;
                    while (num8 < (data.Width - num6))
                    {
                        num7 += (numArray[index + num8] + numArray[index - num8]) * filter[num8];
                        num8++;
                    }
                    for (num8 = data.Width - num6; num8 < filter.Length; num8++)
                    {
                        num7 += (numArray[index + num8] + numArray[index - num8]) * filter[num8];
                    }
                    numArray3[destPtr++] = num7;
                    index++;
                }
            }
        }

        private void FilterPartSymmetricT(object filterJob)
        {
            FilterJob job = (FilterJob) filterJob;
            GrayImage data = job.data;
            float[] numArray = data.Scan0;
            float[] filter = job.filter;
            GrayImage result = job.result;
            int num = filter.Length - 1;
            for (int i = job.start; i < job.end; i++)
            {
                float num6;
                int num7;
                int num3 = i * data.Width;
                int index = num3;
                int num5 = 0;
                while (num5 < num)
                {
                    num6 = numArray[index] * filter[0];
                    num7 = 1;
                    while (num7 < (num5 + 1))
                    {
                        num6 += (numArray[index + num7] + numArray[index - num7]) * filter[num7];
                        num7++;
                    }
                    num7 = num5 + 1;
                    while (num7 < filter.Length)
                    {
                        num6 += (numArray[index + num7] + numArray[index + num7]) * filter[num7];
                        num7++;
                    }
                    result[i, num5] = num6;
                    index++;
                    num5++;
                }
                num5 = num;
                while (num5 < (data.Width - num))
                {
                    num6 = numArray[index] * filter[0];
                    num7 = 1;
                    while (num7 < filter.Length)
                    {
                        num6 += (numArray[index + num7] + numArray[index - num7]) * filter[num7];
                        num7++;
                    }
                    result[i, num5] = num6;
                    index++;
                    num5++;
                }
                for (num5 = data.Width - num; num5 < data.Width; num5++)
                {
                    num6 = numArray[index] * filter[0];
                    num7 = 1;
                    while (num7 < (data.Width - num5))
                    {
                        num6 += (numArray[index + num7] + numArray[index - num7]) * filter[num7];
                        num7++;
                    }
                    for (num7 = data.Width - num5; num7 < filter.Length; num7++)
                    {
                        num6 += (numArray[index - num7] + numArray[index - num7]) * filter[num7];
                    }
                    result[i, num5] = num6;
                    index++;
                }
            }
        }

        public GrayImage GaussianConv(GrayImage data, double std)
        {
            float[] filter = this.GaussianFilter(std);
            return this.Conv2DSeparable(data, filter);
        }

        public float[] GaussianFilter(double std)
        {
            int num5;
            double num = std * std;
            int num3 = (int) Math.Ceiling(Math.Sqrt((-1.0 * num) * Math.Log(0.0099999997764825821)));
            float[] numArray = new float[num3];
            double num4 = -1.0;
            for (num5 = 0; num5 < num3; num5++)
            {
                double num6 = Math.Exp((-0.5 * (num5 * num5)) / num);
                numArray[num5] = (float) num6;
                num4 += 2.0 * num6;
            }
            for (num5 = 0; num5 < num3; num5++)
            {
                numArray[num5] /= (float) num4;
            }
            return numArray;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct FilterJob
        {
            public float[] filter;
            public int start;
            public int end;
            public GrayImage data;
            public GrayImage result;
            public int dataPtr;
            public int destPtr;
        }
    }
}

