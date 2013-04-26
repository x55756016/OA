using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;

namespace SMT.SAAS.Platform.Common
{
    /// <summary>
    /// 获取验证码类
    /// </summary>
    public class IndentifyCodeClass
    {
        /// <summary>
        /// 创建一个IndentifyCodeClass的新实例
        /// </summary>
        public IndentifyCodeClass()
        {

        }

        Random r = new Random(DateTime.Now.Millisecond);
        public void CreatImage(string Text, Image imgsource, int iw, int ih)
        {
            Grid Gx = new Grid(); Canvas cv1 = new Canvas();
            for (int i = 0; i < 6; i++)
            {
                Polyline p = new Polyline();
                for (int ix = 0; ix < r.Next(3, 6); ix++)
                {
                    p.Points.Add(new Point(r.NextDouble() * iw, r.NextDouble() * ih));
                }
                byte[] Buffer = new byte[3]; r.NextBytes(Buffer);
                SolidColorBrush SC = new SolidColorBrush(Color.FromArgb(255, Buffer[0], Buffer[1], Buffer[2]));
                p.Stroke = SC; p.StrokeThickness = 0.5; cv1.Children.Add(p);
            }
            Canvas cv2 = new Canvas();
            int y = 0; int lw = 6; double w = (iw - lw) / Text.Length;
            int h = (int)ih; foreach (char x in Text)
            {
                //byte[] Buffer = new byte[3]; 
                //r.NextBytes(Buffer);
                SolidColorBrush SC = new SolidColorBrush(Colors.White); //Color.FromArgb(255, Buffer[0], Buffer[1], Buffer[2])
                TextBlock t = new TextBlock();
                t.TextAlignment = TextAlignment.Center; t.FontSize = r.Next(h - 3, h);
                t.Foreground = SC;
                t.Text = x.ToString();
                t.Projection = new PlaneProjection()
                {
                    RotationX = r.Next(-30, 30),
                    RotationY = r.Next(-30, 30),
                    RotationZ = r.Next(-10, 10)
                };
                cv2.Children.Add(t);
                Canvas.SetLeft(t, lw / 2 + y * w);
                Canvas.SetTop(t, 0); y++;
            }
            Gx.Children.Add(cv1);
            Gx.Children.Add(cv2);
            Gx.Background = new SolidColorBrush(Color.FromArgb(255, 25, 94, 177));
            System.Windows.Media.Imaging.WriteableBitmap W = new WriteableBitmap(Gx, new TransformGroup());
            W.Render(Gx, new TransformGroup());
            imgsource.Source = W;
        }

        public string CreateIndentifyCode(int count)
        {
            string allchar = "2,3,4,5,6,7,8,A,a,B,b,C,c,D,d,E,e,F,f," + "G,H,h,J,j,K,k,L,M,m,N,n,P,p,Q,q,R,r,S,s," + "T,t,U,u,V,v,W,w,X,x,Y,y";
            string[] allchararray = allchar.Split(',');
            string randomcode = "";
            int temp = -1;
            Random rand = new Random();
            for (int i = 0; i < count; i++)
            {
                if (temp != -1)
                {
                    rand = new Random(i * temp * ((int)DateTime.Now.Ticks));
                }
                int t = rand.Next(allchararray.Length);
                if (temp == t)
                {
                    return CreateIndentifyCode(count);
                }
                temp = t;
                randomcode += allchararray[t];
            } return randomcode;
        }
    }
}
