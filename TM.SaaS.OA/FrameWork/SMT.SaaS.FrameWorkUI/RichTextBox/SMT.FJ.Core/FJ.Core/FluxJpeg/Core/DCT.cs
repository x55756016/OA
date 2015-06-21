namespace FluxJpeg.Core
{
    using System;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;

    public class DCT
    {
        private float[] _temp;
        private static readonly float[,] c = buildC();
        private static readonly float[,] cT = buildCT();
        public double[][] divisors;
        public double[] DivisorsChrominance;
        public double[] DivisorsLuminance;
        private static IDCTFunc dynamicIDCT = null;
        public const int N = 8;
        public int[][] quantum;

        internal DCT()
        {
            this.quantum = new int[2][];
            this.divisors = new double[2][];
            this.DivisorsLuminance = new double[0x40];
            this.DivisorsChrominance = new double[0x40];
            this._temp = new float[0x40];
            dynamicIDCT = dynamicIDCT ?? EmitIDCT();
        }

        public DCT(int quality) : this()
        {
            this.Initialize(quality);
        }

        private static float[,] buildC()
        {
            float[,] numArray = new float[8, 8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    numArray[i, j] = (i == 0) ? 0.3535534f : ((float) (0.5 * Math.Cos(((((2.0 * j) + 1.0) * i) * 3.1415926535897931) / 16.0)));
                }
            }
            return numArray;
        }

        private static float[,] buildCT()
        {
            float[,] numArray = new float[8, 8];
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    numArray[j, i] = c[i, j];
                }
            }
            return numArray;
        }

        private static IDCTFunc EmitIDCT()
        {
            int num2;
            int num3;
            int num4;
            Type[] parameterTypes = new Type[] { typeof(float[]), typeof(float[]), typeof(byte[,]) };
            DynamicMethod method = new DynamicMethod("dynamicIDCT", null, parameterTypes);
            ILGenerator iLGenerator = method.GetILGenerator();
            int num = 0;
            for (num2 = 0; num2 < 8; num2++)
            {
                num3 = 0;
                while (num3 < 8)
                {
                    iLGenerator.Emit(OpCodes.Ldarg_1);
                    iLGenerator.Emit(OpCodes.Ldc_I4_S, (short) num++);
                    num4 = 0;
                    while (num4 < 8)
                    {
                        iLGenerator.Emit(OpCodes.Ldarg_0);
                        iLGenerator.Emit(OpCodes.Ldc_I4_S, (short) ((num2 * 8) + num4));
                        iLGenerator.Emit(OpCodes.Ldelem_R4);
                        iLGenerator.Emit(OpCodes.Ldc_R4, c[num4, num3]);
                        iLGenerator.Emit(OpCodes.Mul);
                        if (num4 != 0)
                        {
                            iLGenerator.Emit(OpCodes.Add);
                        }
                        num4++;
                    }
                    iLGenerator.Emit(OpCodes.Stelem_R4);
                    num3++;
                }
            }
            MethodInfo methodInfo = typeof(DCT).GetMethod("SetValueClipped", BindingFlags.Public | BindingFlags.Static, null, CallingConventions.Standard, new Type[] { typeof(byte[,]), typeof(int), typeof(int), typeof(float) }, null);
            for (num2 = 0; num2 < 8; num2++)
            {
                for (num3 = 0; num3 < 8; num3++)
                {
                    iLGenerator.Emit(OpCodes.Ldarg_2);
                    iLGenerator.Emit(OpCodes.Ldc_I4_S, (short) num2);
                    iLGenerator.Emit(OpCodes.Ldc_I4_S, (short) num3);
                    iLGenerator.Emit(OpCodes.Ldc_R4, (float) 128f);
                    for (num4 = 0; num4 < 8; num4++)
                    {
                        iLGenerator.Emit(OpCodes.Ldarg_1);
                        iLGenerator.Emit(OpCodes.Ldc_I4_S, (short) ((num4 * 8) + num3));
                        iLGenerator.Emit(OpCodes.Ldelem_R4);
                        iLGenerator.Emit(OpCodes.Ldc_R4, cT[num2, num4]);
                        iLGenerator.Emit(OpCodes.Mul);
                        iLGenerator.Emit(OpCodes.Add);
                    }
                    iLGenerator.EmitCall(OpCodes.Call, methodInfo, null);
                }
            }
            iLGenerator.Emit(OpCodes.Ret);
            return (IDCTFunc) method.CreateDelegate(typeof(IDCTFunc));
        }

        internal float[,] FastFDCT(float[,] input)
        {
            float num;
            float num2;
            float num3;
            float num4;
            float num5;
            float num6;
            float num7;
            float num8;
            float num9;
            float num10;
            float num11;
            float num12;
            float num13;
            float num14;
            float num15;
            float num16;
            float num17;
            float num18;
            float num19;
            int num20;
            float[,] numArray = new float[8, 8];
            for (num20 = 0; num20 < 8; num20++)
            {
                for (int i = 0; i < 8; i++)
                {
                    numArray[num20, i] = input[num20, i] - 128f;
                }
            }
            for (num20 = 0; num20 < 8; num20++)
            {
                num = numArray[num20, 0] + numArray[num20, 7];
                num8 = numArray[num20, 0] - numArray[num20, 7];
                num2 = numArray[num20, 1] + numArray[num20, 6];
                num7 = numArray[num20, 1] - numArray[num20, 6];
                num3 = numArray[num20, 2] + numArray[num20, 5];
                num6 = numArray[num20, 2] - numArray[num20, 5];
                num4 = numArray[num20, 3] + numArray[num20, 4];
                num5 = numArray[num20, 3] - numArray[num20, 4];
                num9 = num + num4;
                num12 = num - num4;
                num10 = num2 + num3;
                num11 = num2 - num3;
                numArray[num20, 0] = num9 + num10;
                numArray[num20, 4] = num9 - num10;
                num13 = (num11 + num12) * 0.7071068f;
                numArray[num20, 2] = num12 + num13;
                numArray[num20, 6] = num12 - num13;
                num9 = num5 + num6;
                num10 = num6 + num7;
                num11 = num7 + num8;
                num17 = (num9 - num11) * 0.3826834f;
                num14 = (0.5411961f * num9) + num17;
                num16 = (1.306563f * num11) + num17;
                num15 = num10 * 0.7071068f;
                num18 = num8 + num15;
                num19 = num8 - num15;
                numArray[num20, 5] = num19 + num14;
                numArray[num20, 3] = num19 - num14;
                numArray[num20, 1] = num18 + num16;
                numArray[num20, 7] = num18 - num16;
            }
            for (num20 = 0; num20 < 8; num20++)
            {
                num = numArray[0, num20] + numArray[7, num20];
                num8 = numArray[0, num20] - numArray[7, num20];
                num2 = numArray[1, num20] + numArray[6, num20];
                num7 = numArray[1, num20] - numArray[6, num20];
                num3 = numArray[2, num20] + numArray[5, num20];
                num6 = numArray[2, num20] - numArray[5, num20];
                num4 = numArray[3, num20] + numArray[4, num20];
                num5 = numArray[3, num20] - numArray[4, num20];
                num9 = num + num4;
                num12 = num - num4;
                num10 = num2 + num3;
                num11 = num2 - num3;
                numArray[0, num20] = num9 + num10;
                numArray[4, num20] = num9 - num10;
                num13 = (num11 + num12) * 0.7071068f;
                numArray[2, num20] = num12 + num13;
                numArray[6, num20] = num12 - num13;
                num9 = num5 + num6;
                num10 = num6 + num7;
                num11 = num7 + num8;
                num17 = (num9 - num11) * 0.3826834f;
                num14 = (0.5411961f * num9) + num17;
                num16 = (1.306563f * num11) + num17;
                num15 = num10 * 0.7071068f;
                num18 = num8 + num15;
                num19 = num8 - num15;
                numArray[5, num20] = num19 + num14;
                numArray[3, num20] = num19 - num14;
                numArray[1, num20] = num18 + num16;
                numArray[7, num20] = num18 - num16;
            }
            return numArray;
        }

        internal byte[,] FastIDCT(float[] input)
        {
            byte[,] output = new byte[8, 8];
            dynamicIDCT(input, this._temp, output);
            return output;
        }

        private void Initialize(int quality)
        {
            int num;
            int num2;
            int num4;
            double[] numArray = new double[] { 1.0, 1.387039845, 1.306562965, 1.175875602, 1.0, 0.785694958, 0.5411961, 0.275899379 };
            if (quality <= 0)
            {
                num4 = 1;
            }
            if (quality > 100)
            {
                num4 = 100;
            }
            if (quality < 50)
            {
                num4 = 0x1388 / quality;
            }
            else
            {
                num4 = 200 - (quality * 2);
            }
            int[] table = JpegQuantizationTable.K1Luminance.getScaledInstance(((float) num4) / 100f, true).Table;
            int index = 0;
            for (num = 0; num < 8; num++)
            {
                num2 = 0;
                while (num2 < 8)
                {
                    this.DivisorsLuminance[index] = 1.0 / (((table[index] * numArray[num]) * numArray[num2]) * 8.0);
                    index++;
                    num2++;
                }
            }
            int[] numArray3 = JpegQuantizationTable.K2Chrominance.getScaledInstance(((float) num4) / 100f, true).Table;
            index = 0;
            for (num = 0; num < 8; num++)
            {
                for (num2 = 0; num2 < 8; num2++)
                {
                    this.DivisorsChrominance[index] = 1.0 / (((numArray3[index] * numArray[num]) * numArray[num2]) * 8.0);
                    index++;
                }
            }
            this.quantum[0] = table;
            this.divisors[0] = this.DivisorsLuminance;
            this.quantum[1] = numArray3;
            this.divisors[1] = this.DivisorsChrominance;
        }

        internal int[] QuantizeBlock(float[,] inputData, int code)
        {
            int[] numArray = new int[0x40];
            int index = 0;
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    numArray[index] = (int) Math.Round((double) (inputData[i, j] * this.divisors[code][index]));
                    index++;
                }
            }
            return numArray;
        }

        public static void SetValueClipped(byte[,] arr, int i, int j, float val)
        {
            arr[i, j] = (val < 0f) ? ((byte) 0) : ((val > 255f) ? ((byte) 0xff) : ((byte) (val + 0.5)));
        }

        private delegate void IDCTFunc(float[] input, float[] temp, byte[,] output);
    }
}

