/// <summary>
/// Log No.： 1
/// Modify Desc： 流程平台脚本错误
/// Modifier： 冉龙军
/// Modify Date： 2010-10-18
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
    public partial class StateActive : UserControl
    {
        public delegate void MoveDelegate(StateActive a, MouseEventArgs e);
       
        bool _isMouseCaptured = false ;
 

        System.Windows.Threading.DispatcherTimer _doubleClickTimer;
        public StateActive()
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
        public Point Position
        {
            get
            {
                Point position;

                position = new Point();
                position.Y = (double)this.GetValue(Canvas.TopProperty);
                position.X = (double)this.GetValue(Canvas.LeftProperty);


                return position;
            }
            set
            {

                this.SetValue(Canvas.TopProperty, value.Y);
                this.SetValue(Canvas.LeftProperty, value.X);
              
            }
        }


       
        Point mousePosition;
        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            _isMouseCaptured = true ;
            
            if (_doubleClickTimer.IsEnabled)
            {
                _doubleClickTimer.Stop();
                _isMouseCaptured = false;
                if (this.Name != "StartFlow" && this.Name != "EndFlow")
                _container.StateActiveSet(this.Name);
               
            //    _container.ShowActivitySetting(this);

            }
            else
            {
                _doubleClickTimer.Start();
            //    this.SetValue(Canvas.ZIndexProperty, _container.NextMaxIndex);

                FrameworkElement element = sender as FrameworkElement;
                mousePosition = e.GetPosition(null);
               
                if (null != element)
                {
                    element.CaptureMouse();
                    element.Cursor = Cursors.Hand;
                }
            }     
        }


        private void UserControl_MouseMove(object sender, MouseEventArgs e)
        {
            
            FrameworkElement element = sender as FrameworkElement;
            element.Cursor = Cursors.Hand;
            element.CaptureMouse();
            if (_isMouseCaptured)
            {
                
                if (e.GetPosition(null) == mousePosition)
                    return;
                // bool hadActualMove = true;
                double Y = e.GetPosition(null).Y - mousePosition.Y;
                double X = e.GetPosition(null).X - mousePosition.X;
                double newTop = Y + Position.Y;
                double newLeft = X + Position.X;


                //// MessageBox.Show("gegfeh");
                // 1s 冉龙军
                //double containerWidth = (double)this.Parent.GetValue(Canvas.WidthProperty);
                //double containerHeight = (double)this.Parent.GetValue(Canvas.HeightProperty);
                Canvas can = (Canvas)this.Parent;
                double containerWidth = ((Size)can.DataContext).Width;
                double containerHeight = ((Size)can.DataContext).Height;
                // 1e

                if (newTop <= 0 - this.ActualHeight / 2 || newLeft <= 0 - this.ActualWidth / 2 || newTop >= containerHeight - this.ActualHeight / 2 || newLeft >= containerWidth - this.ActualWidth / 2)
               {
                   element.ReleaseMouseCapture();
                   return;
               }

                this.SetValue(Canvas.TopProperty, newTop);
                this.SetValue(Canvas.LeftProperty, newLeft);

                mousePosition = e.GetPosition(null);

              //  MessageBox.Show(mousePosition.X.ToString());

                _container.SetPos(this);
                

            }
        }
        private void SetPos(string RuleName)//,string ActiveName,Point Pos)
        {
            RuleLine class2 = FindName(RuleName) as RuleLine;
            class2.SetValue(Canvas.TopProperty, (double)200);
        }
        private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseCaptured = false;
        }
    }
}
