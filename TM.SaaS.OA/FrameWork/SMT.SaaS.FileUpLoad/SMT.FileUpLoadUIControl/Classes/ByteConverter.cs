 using System;
using System.Windows.Data;

namespace SMT.FileUpLoad
{
    public class ByteConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string size = "0 KB";

            if (value != null)
            {
              
                double byteCount=0;
                try
                {
                    byteCount = (double)System.Convert.ToDouble(value);
                }
                catch
                { 
                    
                }
                if (byteCount >= 1073741824)
                    size = String.Format("{0:##.##}", byteCount / 1073741824) + " GB";
                else if (byteCount >= 1048576)
                    size = String.Format("{0:##.##}", byteCount / 1048576) + " MB";
                else if (byteCount >= 1024)
                    size = String.Format("{0:##.##}", byteCount / 1024) + " KB";
                else if (byteCount > 0 && byteCount < 1024)
                    size = "1 KB";       

            }

            return size;
        }

      
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// 根据文件的大小转换成可读的中文
        /// </summary>
        /// <param name="byteCount">值</param>
        /// <returns></returns>
        public static string GetSizeName(double byteCount)
        {
            string size = "0 KB";       
                if (byteCount >= 1073741824)
                    size = String.Format("{0:##.##}", byteCount / 1073741824) + " GB";
                else if (byteCount >= 1048576)
                    size = String.Format("{0:##.##}", byteCount / 1048576) + " MB";
                else if (byteCount >= 1024)
                    size = String.Format("{0:##.##}", byteCount / 1024) + " KB";
                else if (byteCount > 0 && byteCount < 1024)
                    size = "1 KB";           

            return size;
        }
        /// <summary>
        /// 根据文件的大小获取字节（3GB、4MB、4KB）
        /// </summary>
        /// <param name="value">3GB、4MB、4KB</param>
        /// <returns></returns>
        public static double GetByte(string value)
        {
            double byteCount = 0.0;

            if (value.IndexOf('.') > 0)
            {
                string name = value.Split('.')[1];
                double v = System.Convert.ToDouble(value.Substring(0, value.Length - 2));
                if (name.ToUpper() == "GB")
                {
                    byteCount = v * 1073741824;
                }
                if (name.ToUpper() == "MB")
                {
                    byteCount = v * 1048576;
                }
                if (name.ToUpper() == "KB")
                {
                    byteCount = v * 1024;
                }
            }
            else
            {
                if (value != "")
                {
                    byteCount = System.Convert.ToDouble(value);
                }
            }
            return byteCount;

        }
        #endregion
    }
}
