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

namespace SMT.Workflow.Platform.UI.OrganizationControl
{
    public partial class Power : UserControl
    {
        public Power()
        {
            // 为初始化变量所必需
            InitializeComponent();
            this.MouseLeftButtonDown += new MouseButtonEventHandler(Power_MouseLeftButtonDown);
        }
        int _id;
        public int InitId
        {
            get { return _id; }
        }

        void Power_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _id++;

            double[] Radius1 = { 0, 0.2, 0.5, 10, 10, 10 };
            double[] Radius2 = { 0, 0.2, 0.5, 0.5, 1.3, 10 };
            try
            {
                ControlBrush.RadiusX = Radius1[_id];
                ControlBrush.RadiusY = Radius2[_id];

                ControlBrush.Center = new Point(0.5, -0.5);
                ControlBrush.GradientStops[0].Color = Colors.Red;
                ControlBrush.GradientStops[1].Color = Colors.White;

                ControlBrush.GradientOrigin = new Point(0.5, 0.5);
            }
            catch (Exception)
            {
                _id = 0;
                ControlBrush.RadiusX = Radius1[_id];
                ControlBrush.RadiusY = Radius2[_id];
            }
        }
    }
}
