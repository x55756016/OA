using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Shapes;
namespace System.Windows.Controls
{
    public sealed class MessageWindow : Window
    {
        private MessageWindow()
        {
            this.DefaultStyleKey = typeof(MessageWindow);
        }
        public static Style GlobalStyle { get; set; }
        public static Style TextBoxStyle { get; set; }
        public static Style ButtonStyle { get; set; }

        public static void Show(object content)
        {
            MessageWindow.Show("Message", content);
        }
        public static void Show(string caption, object content)
        {
            MessageWindow.Show(caption, content, MessageIcon.None);
        }
        public static void Show(string caption, object content, MessageIcon icon)
        {
            MessageWindow.Show(DialogMode.ApplicationModal, null, caption, content, icon);
        }
        public static void Show(DialogMode dialogMode, FrameworkElement container, string caption, object content, MessageIcon icon)
        {
            MessageWindow.Show<object>(dialogMode, container, caption, content, icon, null, null);
        }
        /// <summary>
        /// 模态化应用
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="caption">标题</param>
        /// <param name="content">内容</param>
        /// <param name="icon">标示</param>
        /// <param name="result">结果</param>
        /// <param name="defaultResult">默认结果</param>
        /// <param name="buttons">显示按钮</param>
        public static void Show<TResult>(string caption, object content,
            MessageIcon icon, Action<TResult> result, TResult defaultResult, params TResult[] buttons)
        {
            MessageWindow.Show<TResult>(DialogMode.ApplicationModal, null, caption, content, icon, result, defaultResult, buttons);
        }
        public static void Show<TResult>(DialogMode dialogMode, FrameworkElement container,
            string caption, object content, MessageIcon icon,
            Action<TResult> result, TResult defaultResult, params TResult[] buttons)
        {
            var window = new MessageWindow
            {
                Style = MessageWindow.GlobalStyle
            };

            window.RemainInsideParent = true;
            window.TitleContent = caption;
            

            var grid = new Grid();
            grid.Margin = new Thickness(0);
            grid.Width = 400.0;
            grid.Background = new SolidColorBrush(Colors.Transparent);

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            if (icon != MessageIcon.None)
            {
                var image = new Image
                {
                    Stretch = Stretch.Uniform,
                    Width = 32,
                    VerticalAlignment = VerticalAlignment.Top,
                    Margin = new Thickness(0, 0, 8, 0),
                    //Source = new BitmapImage(new Uri(string.Format("/SMT.SaaS.FrameworkUI;Component/Images/Resources/{0}.png",
                    //    icon), UriKind.Relative))
                    Source= new BitmapImage(new Uri("/SMT.SaaS.FrameworkUI;Component/Images/Tool/edit.png", UriKind.RelativeOrAbsolute))
                };
               // window.IconContent = image;
            }

            # region Buttons

            var stackPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 8, 0, 0),
                Background = new SolidColorBrush(Colors.Transparent)
            };

            var buttonArray = buttons as object[];

            if (buttonArray.Length == 0)
            {
                buttonArray = new[] { "Ok" };
            }

            foreach (var button in buttonArray)
            {
                var _button = new Button()
                {
                    Content = button,
                    Margin = new Thickness(5, 0, 0, 0),
                    Width = 70,
                    Height = 25
                };

                _button.Click += (sender, e) =>
                {
                    window.Result = (TResult)_button.Content;
                    window.Close();
                };

                stackPanel.Children.Add(_button);
                _button.Focus();
            }

            Grid.SetRow(stackPanel, 1);
            Grid.SetColumnSpan(stackPanel, 2);

            grid.Children.Add(stackPanel);

            # endregion

            # region ContentElement

            //显示内容
            var contentElement = content as FrameworkElement;

            if (contentElement.IsNull())
            {

                contentElement = new TextBox
                {
                    IsReadOnly = true,
                    Height=90.0,
                    Style = (Style)Application.Current.Resources["TextBoxStyle"],
                    TextWrapping = TextWrapping.Wrap,
                    Background = new SolidColorBrush(Colors.White),
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Text = content.ToString()
                };
            }

            Grid.SetColumn(contentElement, 1);
            grid.Children.Add(contentElement);

            # endregion

            window.Content = grid;

            window.Show<TResult>(dialogMode, container, defaultResult, result); 
        }
    }
   
    /// <summary>
    /// 没有；信息；感叹；问题；错误；
    /// </summary>
    public enum MessageIcon
    {
        None,
        Information,
        Exclamation,
        Question,
        Error
    }
}
