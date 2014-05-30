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
using FluxJpeg.Core.Encoder;
using FluxJpeg.Core;
using System.Windows.Media.Imaging;
using System.IO;
using System.Text;

namespace SMT.SaaS.FrameworkUI.RichNotepad
{
    public class Utility
    {
        #region 获取富文本值 转化二进制数组方法
        /// <summary>
        /// 获取富文本框的值-向寒咏
        /// </summary>
        /// <param name="box">富文本框</param>
        /// <returns>返回byte[]类型的值</returns>
        public static byte[] GetRichTextBoxContext(RichTextBox box)
        {
            long bytelength = 0;
            object obj = box.DataContext;
            BlockCollection bco = box.Blocks;

            using (MemoryStream ms = new MemoryStream())
            {
                string StrBlocks = string.Empty;
                byte[] byall = null;
                byte[] bylist = null;
                byte[] start = new byte[1024];
                bool isPic = false;
                int s = 0;
                try
                {
                    for (int i = 0; i < box.Blocks.Count; i++)
                    {
                        Paragraph aPara = (Paragraph)box.Blocks[i];

                        byte[] bytes = null;
                        StringBuilder buder = new StringBuilder();
                        for (int j = 0; j < aPara.Inlines.Count; j++)
                        {
                            if (byall != null)
                            {
                                bylist = byall;
                            }
                            if (j == 0)
                            {
                                buder.Append("あ");
                                buder.Append(aPara.FontSize.ToString());
                                buder.Append("卐");
                                buder.Append(aPara.FontFamily.ToString());
                                buder.Append("卐");
                                SolidColorBrush brush = (SolidColorBrush)aPara.Foreground;
                                Color cl = brush.Color;
                                string clstr = cl.ToString();
                                buder.Append(clstr);
                                buder.Append("卐");
                                buder.Append(aPara.FontWeight.ToString());
                                buder.Append("卐");
                                buder.Append(aPara.FontStyle.ToString());
                                buder.Append("卐");
                                buder.Append(aPara.FontStretch.ToString());
                                buder.Append("卐");
                                buder.Append(aPara.TextAlignment.ToString());
                                buder.Append("卐");
                                buder.Append("あ");
                            }
                            TextElement element = (TextElement)aPara.Inlines[j];
                            if (element.GetType().Equals(typeof(Run)))
                            {
                                Run aRun = (Run)element;
                                if (j == 0)
                                {
                                    if (aRun.TextDecorations != null)
                                        buder.Append(TextDecorations.Underline.ToString());
                                    StrBlocks = aRun.Text;
                                    bytes = Encoding.UTF8.GetBytes(buder.ToString() + StrBlocks);
                                    buder = null;
                                }
                                else
                                {
                                    StrBlocks = aRun.Text;
                                    bytes = Encoding.UTF8.GetBytes(StrBlocks);
                                }
                                StrBlocks = "";
                                bytelength += bytes.Length;
                                isPic = false;
                            }
                            if (element.GetType().Equals(typeof(Hyperlink)))
                            {
                                Hyperlink hyperlink = (Hyperlink)element;
                                string hyurl = hyperlink.NavigateUri.ToString();

                                if (j == 0)
                                {
                                    bytes = Encoding.UTF8.GetBytes(buder.ToString() + " ");
                                    bytelength += bytes.Length;
                                    byall = new byte[bytelength + 1024];
                                    if (bylist != null)
                                    {
                                        bylist.CopyTo(byall, 0);
                                    }
                                    start.CopyTo(byall, 0);
                                    if (bylist == null)
                                    {
                                        bytes.CopyTo(byall, 1024);
                                    }
                                    else
                                    {
                                        bytes.CopyTo(byall, bylist.Length);
                                    }
                                    if (byall != null)
                                    {
                                        bylist = byall;
                                    }

                                }

                                bytes = Encoding.UTF8.GetBytes(hyurl);
                                bytelength += bytes.Length;

                            }
                            if (element.GetType().Equals(typeof(InlineUIContainer)))
                            {
                                s++;
                                if (j == 0)
                                {
                                    bytes = Encoding.UTF8.GetBytes(buder.ToString() + " ");
                                    bytelength += bytes.Length;
                                    byall = new byte[bytelength + 1024];
                                    if (bylist != null)
                                    {
                                        bylist.CopyTo(byall, 0);
                                    }
                                    start.CopyTo(byall, 0);
                                    if (bylist == null)
                                    {
                                        bytes.CopyTo(byall, 1024);
                                    }
                                    else
                                    {
                                        bytes.CopyTo(byall, bylist.Length);
                                    }
                                    if (byall != null)
                                    {
                                        bylist = byall;
                                    }

                                }
                                InlineUIContainer inline = (InlineUIContainer)element;
                                BitmapImage bi = (BitmapImage)((System.Windows.Controls.Image)inline.Child).Source;
                                System.Windows.Controls.Image img = (System.Windows.Controls.Image)inline.Child;

                                if (bi.UriSource.ToString() != "")
                                {
                                    string width = img.Width.ToString();
                                    string height = img.Height.ToString();
                                    string urlpass = "ζ" + bi.UriSource.ToString() + "√" + width + "√" + height + "ζ";
                                    bytes = Encoding.UTF8.GetBytes(urlpass);
                                    bytelength += bytes.Length;
                                    byall = new byte[bytelength + 1024];
                                    if (bylist != null)
                                    {
                                        bylist.CopyTo(byall, 0);
                                    }
                                    start.CopyTo(byall, 0);
                                    if (bylist == null)
                                    {
                                        bytes.CopyTo(byall, 1024);
                                    }
                                    else
                                    {
                                        bytes.CopyTo(byall, bylist.Length);
                                    }
                                    if (byall != null)
                                    {
                                        bylist = byall;
                                    }

                                    continue;
                                }

                                //   MediaElement media = new MediaElement();
                                //imagee
                                //media.Source = img.Source;
                                //a.SetSource = bi;
                                //  WriteableBitmap wb = new WriteableBitmap(int.Parse(width.ToString()), int.Parse(height.ToString()));
                                //RotateTransform b = new RotateTransform();
                                //b.Angle = 45;
                                WriteableBitmap wb = new WriteableBitmap(img, null);//将Image对象转换为WriteableBitmap
                                //   WriteableBitmap wb = new WriteableBitmap(img, null);//将Image对象转换为WriteableBitmap


                                bytes = Convert.FromBase64String(GetBase64Image(wb));//得到byte数组
                                byte[] stabt = BitConverter.GetBytes(bytelength);
                                if (s == 1)
                                {
                                    stabt.CopyTo(start, 0);
                                }
                                else
                                {
                                    stabt.CopyTo(start, 8 * s);
                                }
                                bytelength += bytes.Length;
                                byte[] end = BitConverter.GetBytes(bytelength);
                                if (s == 1)
                                {
                                    end.CopyTo(start, 8);
                                }
                                else
                                {
                                    s++;
                                    end.CopyTo(start, 8 * s);
                                }
                                StrBlocks = "";
                                isPic = true;
                            }
                            byall = new byte[bytelength + 1024];
                            if (bylist != null)
                            {
                                if (isPic)
                                {
                                    bylist.CopyTo(byall, 0);
                                    start.CopyTo(byall, 0);
                                }
                                else
                                {
                                    bylist.CopyTo(byall, 0);
                                }
                            }
                            if (j == 0)
                            {
                                start.CopyTo(byall, 0);
                                if (bylist == null)
                                {
                                    bytes.CopyTo(byall, 1024);
                                }
                                else
                                {
                                    bytes.CopyTo(byall, bylist.Length);
                                }
                            }
                            else
                            {
                                bytes.CopyTo(byall, bylist.Length);
                            }

                        }
                        //  StrBlocks = "\n";
                    }
                }
                catch (Exception ex)
                {
                    byall = null;
                    throw ex;
                }
                return byall;
            }
        }
        #endregion

        #region 转化流文件操作

        /// <summary>
        /// 将WriteableBitmap转化为base64位字符串  
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        private static string GetBase64Image(WriteableBitmap bitmap)
        {
            int width = bitmap.PixelWidth;
            int height = bitmap.PixelHeight;
            int bands = 3;
            byte[][,] raster = new byte[bands][,];

            for (int i = 0; i < bands; i++)
            {
                raster[i] = new byte[width, height];
            }

            for (int row = 0; row < height; row++)
            {
                for (int column = 0; column < width; column++)
                {
                    int pixel = bitmap.Pixels[width * row + column];
                    raster[0][column, row] = (byte)(pixel >> 16);
                    raster[1][column, row] = (byte)(pixel >> 8);
                    raster[2][column, row] = (byte)pixel;
                }
            }

            ColorModel model = new ColorModel { colorspace = ColorSpace.RGB };
            FluxJpeg.Core.Image img = new FluxJpeg.Core.Image(model, raster);
            MemoryStream stream = new MemoryStream();
            JpegEncoder encoder = new JpegEncoder(img, 100, stream);
            encoder.Encode();

            stream.Seek(0, SeekOrigin.Begin);
            byte[] binaryData = new Byte[stream.Length];
            long bytesRead = stream.Read(binaryData, 0, (int)stream.Length);

            string base64String =
                    System.Convert.ToBase64String(binaryData,
                                                  0,
                                                  binaryData.Length);

            return base64String;

        }

        /// <summary>
        /// 将BitmapImag转化为二进制流
        /// </summary>
        /// <param name="bmp"></param>
        /// <returns></returns>
        public static byte[] BitmapImageToByteArray(BitmapImage bmp)
        {
            byte[] byteArray = null;

            try
            {
                Stream sMarket = null;

                if (sMarket != null && sMarket.Length > 0)
                {
                    //很重要，因为Position经常位于Stream的末尾，导致下面读取到的长度为0。 
                    sMarket.Position = 0;

                    using (BinaryReader br = new BinaryReader(sMarket))
                    {
                        byteArray = br.ReadBytes((int)sMarket.Length);
                    }
                }
            }
            catch
            {
                //other exception handling 
            }

            return byteArray;
        }

        /// 将Image对象转化成二进制流 
        ///  </summary> 
        ///  <param name="image"> </param> 
        ///  <returns> </returns> 
        public byte[] ImageToByteArray(System.Windows.Controls.Image image)
        {
            //实例化流 
            MemoryStream imageStream = new MemoryStream();
            //将图片的实例保存到流中           

            //保存流的二进制数组 
            byte[] imageContent = new Byte[imageStream.Length];

            imageStream.Position = 0;
            //将流泻如数组中 
            imageStream.Read(imageContent, 0, (int)imageStream.Length);

            return imageStream.ToArray();

        }

        /// <summary>
        /// 将byte[]转化为整形数组
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public static int[] Byte64toLong(byte[] start)
        {
            byte[] bylong = new byte[8];
            int[] val = new int[start.Length / 8];

            try
            {
                //  Array arr = new Array[start.Length / 8];
                for (int i = 0; i < start.Length / 8; i++)
                {
                    Array.Copy(start, 8 * i, bylong, 0, 8);
                    long a = BitConverter.ToInt64(bylong, 0);

                    if (i == 1)
                    {
                        if (a == 0)
                        {
                            val = null;
                            break;
                        }
                    }
                    if (i > 0)
                    {
                        if (a == 0)
                        {
                            break;
                        }
                    }
                    val[i] = (int)a;
                }
            }
            catch (Exception e)
            {
                val = null;
            }


            return val;

        }

        /// <summary>
        /// 图片控件加载图片方法
        /// </summary>
        /// <param name="image"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static System.Windows.Controls.Image ByteToImage(byte[] image, double width, double height)
        {
            System.Windows.Controls.Image img = new System.Windows.Controls.Image();
            img.Stretch = Stretch.Uniform;

            BitmapImage bi = new BitmapImage();
            try
            {
                bi.SetSource(new MemoryStream(image));
                img.Source = bi;
                img.Tag = bi.UriSource.ToString();
                img.Width = bi.PixelWidth;//图片宽度
                img.Height = bi.PixelHeight;//图片高度
            }
            catch
            {
                return null;
            }

            return img;
        }

        private static void ReturnFocus(RichTextBox box)
        {
            if (box != null)
                box.Focus();
        }

        #endregion

        #region 富文本取值 将Byte数组 转化为文字和图片
        /// <summary>
        /// 富文本框赋值-向寒咏
        /// </summary>
        /// <param name="box">富文本框</param>
        /// <param name="RichBoxData">保存的值-Byte[]类型</param>
        public static void SetRichTextBoxData(RichTextBox box, byte[] RichBoxData)
        {
            box.Blocks.Clear();
            if (RichBoxData == null || RichBoxData.Length == 0)
            {
                return;
            }
            byte[] start = new byte[1024];   //图片定位数组
            if (RichBoxData.Length > 1024)
                Array.Copy(RichBoxData, 0, start, 0, 1024);

            long starleng = 0;     //开始截取的位置
            long endleng = 0;     //结束截取的位置
            long endy = 0;         //获取上次结束的位置
            bool isendtext = false;  //不存在图片
            int[] lengst = Byte64toLong(start);  //转化定位数组
            if (RichBoxData.Length > 1024)
            {
                for (int x = 1024; x < RichBoxData.Length; x++)   //内容进行循环   位置从开始匹配
                {
                    if (lengst != null)
                    {
                        endy = endleng;
                        for (int a = 0; a < lengst.Length; a++)   //定位数组进行循环
                        {
                            if (a % 2 == 0)
                            {
                                starleng = long.Parse(lengst[a].ToString());  //获取开始出现图片位置
                                continue;
                            }
                            else
                            {
                                endleng = long.Parse(lengst[a].ToString());   //获取结束图片位置

                                if (endleng == 0)
                                {
                                    isendtext = true;
                                }
                            }
                            if (starleng == x - 1024 && isendtext == false)                      //当前位置是图片位置 进行处理
                            {
                                byte[] imageByte = new byte[endleng - starleng];

                                Array.Copy(RichBoxData, (int)starleng + 1024, imageByte, 0, (int)(endleng - starleng));
                                SetImageBindBox(box, imageByte);  //控件绑定
                                x = (int)endleng + 1024;
                                if (x == RichBoxData.Length - 2)
                                {
                                    x++;
                                }

                            }  // 处理文字信息
                            else
                            {
                                byte[] textByte = null;
                                if (starleng == 0 && isendtext == true)          //所有都是文字处理方式
                                {
                                    textByte = new byte[RichBoxData.Length - x];
                                    if (RichBoxData.Length == x)
                                    {
                                        break;
                                    }
                                    Array.Copy(RichBoxData, x, textByte, 0, (int)(RichBoxData.Length - x));
                                }

                                if (starleng > x - 1024)    // 当前位置小于开始图片位置
                                {
                                    textByte = new byte[starleng - x + 1024];
                                    Array.Copy(RichBoxData, x, textByte, 0, (int)(starleng - x + 1024));
                                }

                                SetTextBindBox(box, textByte, 1);
                                //using (MemoryStream stream = new MemoryStream(textByte))
                                //{
                                //    using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                                //    {
                                //        string Xaml = reader.ReadToEnd();
                                //        Run myRun = new Run();
                                //        myRun.Text = Xaml;
                                //        box.Selection.Insert(myRun);

                                //    }

                                //}
                                if (starleng == 0)          //所有都是文字处理方式  没有图片时跳出循环
                                {
                                    x = RichBoxData.Length - 1;
                                    break;
                                }
                                if (starleng > x - 1024)    //当当前位置
                                {
                                    x = 1024 + (int)starleng;
                                    byte[] imageByte = new byte[endleng - starleng];
                                    Array.Copy(RichBoxData, x, imageByte, 0, (int)(endleng - starleng));
                                    SetImageBindBox(box, imageByte);  //控件绑定
                                    x = (int)endleng + 1024;
                                    if (x == RichBoxData.Length - 2)
                                    {
                                        x++;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        #region 判断没有图片的时候
                        if (starleng == 0 && endleng == 0)
                        {
                            byte[] strByte = new byte[RichBoxData.Length - 1024];
                            Array.Copy(RichBoxData, 1024, strByte, 0, RichBoxData.Length - 1024);
                            SetTextBindBox(box, strByte, 1);

                            break;
                        }
                        #endregion
                    }

                }
            }
            else
            {
                //SetTextBindBox(box, RichBoxData,1);
                using (MemoryStream stream = new MemoryStream(RichBoxData))
                {
                    using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                    {
                        string Xaml = reader.ReadToEnd();
                        Run myRun = new Run();
                        myRun.Text = Xaml;
                        box.Selection.Insert(myRun);

                    }

                }
            }
            //box.SelectAll();
            //box.Selection.ApplyPropertyValue(Run.FontSizeProperty, "12");
            TextPointer startPointer = box.ContentStart.GetNextInsertionPosition(LogicalDirection.Forward);
            TextPointer MyTP1 = startPointer.GetPositionAtOffset(0, LogicalDirection.Forward);
            box.Selection.Select(startPointer, MyTP1);
            box.Focus();

        }
        #endregion

        #region 富文本绑定 图片和文本
        /// <summary>
        /// 富文本绑定图片数组
        /// </summary>
        /// <param name="box"></param>
        /// <param name="imageByte"></param>
        public static void SetImageBindBox(RichTextBox box, byte[] imageByte)
        {
            InlineUIContainer container = new InlineUIContainer();
            container.Child = ByteToImage(imageByte, 200, 150);

            //box.Selection.Insert(container);

            Paragraph aPara = (Paragraph)box.Blocks[box.Blocks.Count - 1];
            aPara.Inlines.Add(container);
            //TextElement element = (TextElement)aPara.Inlines[0];
            //element = (TextElement)container;
            //box.Selection.Insert(aPara);

        }
        //图片转化
        public static System.Windows.Controls.Image CreateImageFromUri(Uri URI, double width, double height)
        {
            System.Windows.Controls.Image img = new System.Windows.Controls.Image();
            img.Stretch = Stretch.Uniform;

            img.Width = width;
            img.Height = height;
            BitmapImage bi = new BitmapImage(URI);
            img.Source = bi;
            img.Tag = bi.UriSource.ToString();
            return img;
        }

        private static void GetUrlOrText(RichTextBox box, Paragraph aPara, string str)
        {

            string[] tt = str.ToString().Split('ζ');
            if (box.Blocks.Count != 0)
            {
                aPara = (Paragraph)box.Blocks[box.Blocks.Count-1];
            }
            else
            {
                
            }

            if (tt.Length == 1 || tt[0].ToString() != "")
            {
                Run myRun = new Run();
                myRun.Text = tt[0].ToString();

                aPara.Inlines.Add(myRun);
               // box.Blocks.Add(mm);
             
               // aPara = (Paragraph)box.Blocks[0];
            }
            if (tt.Length > 2)
            {
                for (int z = 0; z < (tt.Length + 1) / 3; z++)
                {
                    InlineUIContainer container = new InlineUIContainer();
                    Run myRun = new Run();

                    string str1 = tt[z * 3 - z].ToString();
                    if (str1 != "" && z == 0)
                    {
                        myRun.Text = str1;
                        aPara.Inlines.Add(myRun);
                    }
                    string url = tt[z * 3 - z + 1].ToString();
                    string[] urlarr = url.Split('√');

                    //添加图片插入到InlineUIContainer之中 
                    Uri imageUri = new Uri(urlarr[0], UriKind.RelativeOrAbsolute);
                    double width = double.Parse(urlarr[1]);
                    double height = double.Parse(urlarr[2]);
                    container.Child = CreateImageFromUri(imageUri, width, height);
                    aPara.Inlines.Add(container);

                    string str2 = tt[z * 3 - z + 2].ToString();
                    if (str2 != "")
                    {
                        Run myRun2 = new Run();
                        myRun2.Text = str2;
                        aPara.Inlines.Add(myRun2);
                    }

                }
            }
        }

        /// <summary>
        /// 富文本框绑定 文本数组
        /// </summary>
        /// <param name="box"></param>
        /// <param name="RichBoxData"></param>
        public static void SetTextBindBox(RichTextBox box, byte[] RichBoxData, int type)
        {

            using (MemoryStream stream = new MemoryStream(RichBoxData))
            {
                using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                {
                    string Xaml = reader.ReadToEnd();
                    string[] arr = null;
                    try
                    {
                        arr = Xaml.Split('あ');
                        if (arr.Length == 1 || arr[0].ToString() != "")
                        {
                            Paragraph aPara = new Paragraph();
                            GetUrlOrText(box, aPara, arr[0].ToString());
                        }
                        for (int i = 1; i < arr.Length; i += 2)
                        {
                            Paragraph myPara = new Paragraph();
                            //   Run myRun = new Run();
                            string but = arr[i].ToString();
                            string[] lad = but.Split('卐');
                            myPara.FontSize = double.Parse(lad[0].ToString());
                            FontFamily fot = new FontFamily(lad[1].ToString());
                            string color = lad[2].ToString();
                            myPara.FontFamily = fot;
                            Color cl = Color.FromArgb(byte.Parse(color.Substring(1, 2), System.Globalization.NumberStyles.HexNumber),
                                                      byte.Parse(color.Substring(3, 2), System.Globalization.NumberStyles.HexNumber),
                                                      byte.Parse(color.Substring(5, 2), System.Globalization.NumberStyles.HexNumber),
                                                      byte.Parse(color.Substring(7, 2), System.Globalization.NumberStyles.HexNumber));
                            SolidColorBrush brush = new SolidColorBrush(cl);
                            myPara.Foreground = brush;
                            if (lad[3].ToString() == "Normal")
                            {
                                myPara.FontWeight = FontWeights.Normal;
                            }
                            else
                            {
                                myPara.FontWeight = FontWeights.Bold;
                            }
                            if (lad[4].ToString() == "Normal")
                            {
                                myPara.FontStyle = FontStyles.Normal;
                            }
                            else
                            {
                                myPara.FontStyle = FontStyles.Italic;
                            }
                            FontStretch a = new FontStretch();
                            myPara.FontStretch = a;
                            if (lad[6].ToString() == "Left")
                            {
                                myPara.TextAlignment = TextAlignment.Left;
                            }
                            if (lad[6].ToString() == "Right")
                            {
                                myPara.TextAlignment = TextAlignment.Right;
                            }
                            if (lad[6].ToString() == "Center")
                            {
                                myPara.TextAlignment = TextAlignment.Center;
                            }
                            box.Blocks.Add(myPara);
                            GetUrlOrText(box, myPara, arr[i + 1].ToString());   //插入图片Or文字
                            
                            //if (type == 1)
                            //{
                            //    myPara.Inlines.Add(myRun);

                            //}
                            //else
                            //{
                            //    box.Selection.Insert(myRun);
                            //}
                            //box.Blocks.Add(myPara);

                        }
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }
                }

            }
        }
        #endregion
    }
}
