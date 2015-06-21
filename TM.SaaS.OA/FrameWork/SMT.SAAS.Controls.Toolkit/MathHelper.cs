using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SMT.SAAS.Controls.Toolkit
{
    public static class MathHelper
    {
        // Methods
        public static double DegreesToRadians(double degrees)
        {
            return (degrees * 0.017453292519943295);
        }

        public static double GetAngleFromOffset(Point offset)
        {
            double num = Math.Abs(offset.Y);
            double num2 = Math.Abs(offset.X);
            if (offset.Y < 0.0)
            {
                num = -num;
            }
            double d = RadiansToDegrees(Math.Atan(num / num2));
            if (double.IsNaN(d))
            {
                return 0.0;
            }
            if (offset.X < 0.0)
            {
                d = 90.0 + (90.0 - d);
            }
            return d;
        }

        public static Point GetOffset(double angle, double distance)
        {
            double x = Math.Cos(DegreesToRadians(angle)) * distance;
            return new Point(x, Math.Tan(DegreesToRadians(angle)) * x);
        }

        public static double RadiansToDegrees(double radians)
        {
            return (radians * 57.295779513082323);
        }
    }

}
