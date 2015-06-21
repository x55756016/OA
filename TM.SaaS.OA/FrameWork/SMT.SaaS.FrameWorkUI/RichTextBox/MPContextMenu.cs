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
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using SMT.SaaS.FrameworkUI.RichNotepad;

namespace SMT.SaaS.FrameworkUI.RichNotepad
{
    public class MPContextMenu : Dialog
    {
        RichMainPage mp;

        public MPContextMenu(RichMainPage mp)
        {
            this.mp = mp;
        }

        protected override FrameworkElement GetContent()
        {
            Border border = new Border() { BorderBrush = new SolidColorBrush(Color.FromArgb(255, 167, 171, 176)), CornerRadius=new CornerRadius(2), BorderThickness = new Thickness(1), Background = new SolidColorBrush(Colors.White) };
            border.Effect = new DropShadowEffect() { BlurRadius = 3, Color = Color.FromArgb(255, 230, 227, 236) };

            Grid grid = new Grid() { Margin = new Thickness(1) };
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(25) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(105) });

            grid.Children.Add(new Rectangle() { Fill = new SolidColorBrush(Color.FromArgb(255, 233, 238, 238))});
            grid.Children.Add(new Rectangle() { Fill = new SolidColorBrush(Color.FromArgb(255, 226, 228, 231)), HorizontalAlignment = HorizontalAlignment.Right, Width = 1 });

            Button roButton = new Button() { Height=22, Margin = new Thickness(0, 0, 0, 0), HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Top, HorizontalContentAlignment = HorizontalAlignment.Left };
            roButton.Style = Application.Current.Resources["ContextMenuButton"] as Style;
            roButton.Click += ro_MouseLeftButtonUp;
            Grid.SetColumnSpan(roButton, 2);

            StackPanel sp = new StackPanel() { Orientation= Orientation.Horizontal };

            Image roImage = new Image() { HorizontalAlignment = HorizontalAlignment.Left, Width = 16, Height = 16, Margin = new Thickness(1, 0, 0, 0) };
            if (mp.rtb.IsReadOnly)
            {
                roImage.Source = new BitmapImage(new Uri("/RichNotepad;component/images/edit_16.png", UriKind.RelativeOrAbsolute));
            }
            else
            {
                roImage.Source = new BitmapImage(new Uri("/RichNotepad;component/images/view.png", UriKind.RelativeOrAbsolute));
            }
            sp.Children.Add(roImage);

            TextBlock roText = new TextBlock() { HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(16, 0, 0, 0) };
            if (mp.rtb.IsReadOnly)
            {
                roText.Text = "Edit Mode";
            }
            else
            {
                roText.Text = "View Mode";
            }
            sp.Children.Add(roText);

            roButton.Content = sp;

            grid.Children.Add(roButton);  

            border.Child = grid;

            return border;
        }

        void ltr_MouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            mp.btnRTL_Checked(null, null);
            Close();
        }

        void ro_MouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            //mp.btnRO_Checked(null, null);
           // mp.rtb.ToggleReadOnly();
            Close();
        }

        protected override void OnClickOutside()
        {
            Close();
        }
    }
}