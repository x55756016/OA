
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

namespace SMT.SaaS.FrameworkUI.RichNotepad
{
    public class RTBContextMenu : Dialog
    {
        RichTextBox rtb;

        public RTBContextMenu(RichTextBox rtb)
        {
            this.rtb = rtb;
        }

        protected override FrameworkElement GetContent()
        {
            //<Border BorderBrush="#FFA7ABB0" BorderThickness="1" HorizontalAlignment="Left" Margin="101,71,0,0" Grid.Row="1" Width="85" Height="106" VerticalAlignment="Top" CornerRadius="2" Background="White">
            //    <Border.Effect>
            //        <DropShadowEffect BlurRadius="3" Color="#FFE6E3EC"/>
            //    </Border.Effect>
            //    <Grid Margin="4">
            //        <Grid.ColumnDefinitions>
            //            <ColumnDefinition Width="27"/>
            //            <ColumnDefinition Width="102"/>
            //        </Grid.ColumnDefinitions>
            //        <Image HorizontalAlignment="Left" VerticalAlignment="Top" Source="Images/Cut.png" Width="16" Height="16" Margin="4,7,0,0"/>
            //        <Rectangle Fill="#FFE2E4E7" HorizontalAlignment="Right" Margin="0,1" Width="1"/>
            //        <Image HorizontalAlignment="Left" Height="16" Margin="4,35,0,0" Source="Images/Copy.png" VerticalAlignment="Top" Width="16"/>
            //        <Image HorizontalAlignment="Left" Margin="4,65,0,0" Source="Images/Paste.png" Width="16" Height="16" VerticalAlignment="Top"/>
            //        <Button Content="Cut" Grid.Column="1" Margin="4,4,0,0" VerticalAlignment="Top" HorizontalAlignment="Left"/>
            //        <Button Content="Copy" Grid.Column="1" HorizontalAlignment="Left" Margin="4,32,0,0" VerticalAlignment="Top"/>
            //        <Button Content="Paste" Grid.Column="1" HorizontalAlignment="Left" Margin="4,62,0,0" d:LayoutOverrides="Height" VerticalAlignment="Top"/>
            //    </Grid>
            //</Border>

            Border border = new Border() { BorderBrush = new SolidColorBrush(Color.FromArgb(255, 167,171,176)), BorderThickness = new Thickness(1), Background = new SolidColorBrush(Colors.White) };
            border.Effect = new DropShadowEffect() { BlurRadius = 3, Color = Color.FromArgb(255, 230, 227, 236) };

            Grid grid = new Grid() { Margin = new Thickness(1) };
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(25) });
            grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(105) });

            grid.Children.Add(new Rectangle() { Fill = new SolidColorBrush(Color.FromArgb(255, 233, 238, 238)) });
            grid.Children.Add(new Rectangle() { Fill = new SolidColorBrush(Color.FromArgb(255, 226, 228, 231)), HorizontalAlignment = HorizontalAlignment.Right, Width = 1 });
            
            //cut
            Button cutButton = new Button() { Height = 22, Margin = new Thickness(0, 0, 0, 0), HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Top, HorizontalContentAlignment = HorizontalAlignment.Left };
            cutButton.Style = Application.Current.Resources["ContextMenuButton"] as Style;
            cutButton.Click += cut_MouseLeftButtonUp;
            Grid.SetColumnSpan(cutButton, 2);

            StackPanel sp = new StackPanel() { Orientation = Orientation.Horizontal };

            Image cutImage = new Image() { HorizontalAlignment = HorizontalAlignment.Left, Width = 16, Height = 16, Margin = new Thickness(1, 0, 0, 0) };
            cutImage.Source = new BitmapImage(new Uri("/RichNotepad;component/images/cut.png", UriKind.RelativeOrAbsolute));
            sp.Children.Add(cutImage);

            TextBlock cutText = new TextBlock() { Text = "Cut", HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(16, 0, 0, 0) };
            sp.Children.Add(cutText);

            cutButton.Content = sp;

            grid.Children.Add(cutButton);

            //copy
            Button copyButton = new Button() { Height = 22, Margin = new Thickness(0, 24, 0, 0), HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Top, HorizontalContentAlignment = HorizontalAlignment.Left };
            copyButton.Style = Application.Current.Resources["ContextMenuButton"] as Style;
            copyButton.Click += copy_MouseLeftButtonUp;
            Grid.SetColumnSpan(copyButton, 2);

            sp = new StackPanel() { Orientation = Orientation.Horizontal };

            Image copyImage = new Image() { HorizontalAlignment = HorizontalAlignment.Left, Width = 16, Height = 16, Margin = new Thickness(1, 0, 0, 0) };
            copyImage.Source = new BitmapImage(new Uri("/RichNotepad;component/images/copy.png", UriKind.RelativeOrAbsolute));
            sp.Children.Add(copyImage);

            TextBlock copyText = new TextBlock() { Text = "Copy", HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(16, 0, 0, 0) };
            sp.Children.Add(copyText);

            copyButton.Content = sp;

            grid.Children.Add(copyButton);          

            //paste
            Button pasteButton = new Button() { Height = 22, Margin = new Thickness(0, 48, 0, 0), HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Top, HorizontalContentAlignment = HorizontalAlignment.Left };
            pasteButton.Style = Application.Current.Resources["ContextMenuButton"] as Style;
            pasteButton.Click += paste_MouseLeftButtonUp;
            Grid.SetColumnSpan(pasteButton, 2);

            sp = new StackPanel() { Orientation = Orientation.Horizontal };

            Image pasteImage = new Image() { HorizontalAlignment = HorizontalAlignment.Left, Width = 16, Height = 16, Margin = new Thickness(1, 0, 0, 0) };
            pasteImage.Source = new BitmapImage(new Uri("/RichNotepad;component/images/paste.png", UriKind.RelativeOrAbsolute));
            sp.Children.Add(pasteImage);

            TextBlock pasteText = new TextBlock() { Text = "Paste", HorizontalAlignment = HorizontalAlignment.Left, Margin = new Thickness(16, 0, 0, 0) };
            sp.Children.Add(pasteText);

            pasteButton.Content = sp;

            grid.Children.Add(pasteButton);

            border.Child = grid;

            return border;
        }

        void cut_MouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(rtb.Selection.Text);
            rtb.Selection.Text = "";
            Close();
        }

        void copy_MouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(rtb.Selection.Text);
            Close();
        }

        void paste_MouseLeftButtonUp(object sender, RoutedEventArgs e)
        {
            rtb.Selection.Text = Clipboard.GetText();
            Close();
        }

        protected override void OnClickOutside()
        {
            Close();
        }
    }
}