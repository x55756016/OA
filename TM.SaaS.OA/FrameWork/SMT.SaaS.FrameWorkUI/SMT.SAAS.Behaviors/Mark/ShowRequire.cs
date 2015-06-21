/******************************
 * 功  能：行为加载必填项*
 * 时  间：2010年8月6日
 * 编辑者：王玲
 * ***************************/
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
using System.Windows.Interactivity;

namespace SMT.SAAS.Behaviors
{
    public class ShowRequire : Behavior<FrameworkElement>
    {
        Control _txt;
        protected override void OnAttached()
        {
            base.OnAttached();
            this.AssociatedObject.Loaded += new RoutedEventHandler(AssociatedObject_Loaded);

            if (AssociatedObject is Control)
            {
                _txt =(Control)AssociatedObject;
                
                _txt.IsEnabledChanged += new DependencyPropertyChangedEventHandler(_txt_IsEnabledChanged);
            }
        }

        //更改控件可用事件
        void _txt_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (AssociatedObject is Control)
            {
                _txt = (Control)AssociatedObject;
                if (_txt.IsEnabled == true)
                {
                    _redstar.Visibility = Visibility.Visible;
                }
                else
                {
                    _redstar.Visibility = Visibility.Collapsed;
                }
            }
        }

        //定义红色标记必填项
        RequireMark _redstar = new RequireMark();
        void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            if (AssociatedObject is Control)
            {
                _txt = (Control)AssociatedObject;
                SetMark();
                if (_txt.IsEnabled == true)
                {
                    _redstar.Visibility = Visibility.Visible;
                }
                else
                {
                    _redstar.Visibility = Visibility.Collapsed;
                }
            }
        }

        #region 设置必填项的位置
        public void SetMark()
        {
            Panel panel = AssociatedObject.Parent as Panel;
            if (panel.Children.Contains(_redstar) == false)
            {
                _redstar.Visibility = Visibility.Visible;
                panel.Children.Add(_redstar);
                _redstar.VerticalAlignment = VerticalAlignment.Top;
                _redstar.Margin = new Thickness(2, 8, 0, 0);
                //父对象为Grid，获取对象所在的位置，并加入标示
                if (panel is Grid)
                {

                    var col = Grid.GetColumn(AssociatedObject);
                    var row = Grid.GetRow(AssociatedObject);

                    var mRight = AssociatedObject.Margin;

                    Grid.SetColumn(_redstar, col);
                    Grid.SetRow(_redstar, row);
                    Grid.SetColumnSpan(_redstar, Grid.GetColumnSpan(AssociatedObject));
                    Grid.SetRowSpan(_redstar, Grid.GetRowSpan(AssociatedObject));
                }

                var w = AssociatedObject.Width;
                var objmarginwidth = AssociatedObject.Margin.Right;

                //填充方式
                if (Double.NaN.Equals(w) == true)
                {
                    var _left = AssociatedObject.Margin.Left;
                    var _top = AssociatedObject.Margin.Top;
                    var _right = AssociatedObject.Margin.Right;
                    var _bottom = AssociatedObject.Margin.Bottom;

                    var Aright = AssociatedObject.Margin.Right -10;
                   // AssociatedObject.Margin = new Thickness(_left, _top, Aright, _bottom);
                   // objmarginwidth = AssociatedObject.Margin.Right;

                    _redstar.HorizontalAlignment = HorizontalAlignment.Right;
                    _redstar.Margin = new Thickness(0, 8, Aright, 0);
                }
                //固定方式
                else
                {
                    double _objectwidth = AssociatedObject.Width;
                    double _objectmaginleft = AssociatedObject.Margin.Left;
                    if (AssociatedObject.HorizontalAlignment == HorizontalAlignment.Center)
                    {
                        _redstar.HorizontalAlignment = HorizontalAlignment.Center;
                        var objmargin = _objectwidth + 10 + _objectmaginleft;
                        _redstar.Margin = new Thickness(objmargin, 8, 0, 0);
                    }
                    else if (AssociatedObject.HorizontalAlignment == HorizontalAlignment.Right)
                    {
                        var _left = AssociatedObject.Margin.Left;
                        var _top = AssociatedObject.Margin.Top;
                        var _right = AssociatedObject.Margin.Right;
                        var _bottom = AssociatedObject.Margin.Bottom;

                        var Aright = AssociatedObject.Margin.Right + 0;
                        AssociatedObject.Margin = new Thickness(_left, _top, Aright, _bottom);

                        _redstar.HorizontalAlignment = HorizontalAlignment.Right;
                        _redstar.Margin = new Thickness(0, 8, -15, 0);
                    }

                    else
                    {
                        _redstar.HorizontalAlignment = HorizontalAlignment.Left;
                        double objmargin = _objectwidth + _objectmaginleft + 5;
                        _redstar.Margin = new Thickness(objmargin, 8, 0, 0);
                    }
                }
            }
        }

        #endregion

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }
    }
}
