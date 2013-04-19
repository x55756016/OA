using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SMT.HRM.UI.Active
{
    public partial class RuleLine : UserControl
    {
        public string StrStartActive="",StrEndActive="";

        public RuleConditions ruleCoditions;

        IContainer _container;
        public IContainer Container
        {
            get
            {
                return _container;
            }
            set
            {
                _container = value;
            }
        }
        System.Windows.Threading.DispatcherTimer _doubleClickTimer;
        public RuleLine()
        {
            InitializeComponent();
            _doubleClickTimer = new System.Windows.Threading.DispatcherTimer();
            _doubleClickTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            _doubleClickTimer.Tick += new EventHandler(DoubleClick_Timer);
        }
        void DoubleClick_Timer(object sender, EventArgs e)
        {
            _doubleClickTimer.Stop();
        }
        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_doubleClickTimer.IsEnabled)
            {
                _doubleClickTimer.Stop();
                //MessageBox.Show("fwsgw");
                //_isMouseCaptured = false;
                //if (this.Name != "StartFlow" && this.Name != "EndFlow")
                    _container.RuleActiveSet(this.Name);

                //    _container.ShowActivitySetting(this);

            }
            else
            {
                _doubleClickTimer.Start();
                //    this.SetValue(Canvas.ZIndexProperty, _container.NextMaxIndex);

                FrameworkElement element = sender as FrameworkElement;
              //  mousePosition = e.GetPosition(null);

                if (null != element)
                {
                    element.CaptureMouse();
                    element.Cursor = Cursors.Hand;
                }
            }
        }

        ///　<summary>
        ///　根据直线的起始点和结束点的坐标设置箭头的旋转角度
        ///　</summary>
        ///　<param　name="beginPoint"></param>
        ///　<param　name="endPoint"></param>
        public void SetAngleByPoint( Point endPoint)
        {


            Foot.X1 = endPoint.X;
            Foot.Y1 = endPoint.Y;
            Foot.X2 = endPoint.X;
            Foot.Y2 = endPoint.Y+2;
            //Foot.StrokeEndLineCap = PenLineCap.Triangle;
            //Foot.StrokeThickness = 9;
  

        }

    }
}
