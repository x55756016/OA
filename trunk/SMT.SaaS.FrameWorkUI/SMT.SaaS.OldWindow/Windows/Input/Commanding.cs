using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace System.Windows.Input
{
	public class Commanding
	{
		private Commanding() {}

		public static readonly DependencyProperty CommandProperty =
			DependencyProperty.RegisterAttached("Command", typeof(ICommand),
			typeof(Commanding), new PropertyMetadata(
			new PropertyChangedCallback(Commanding.CommandPropertyChanged)));

		public static ICommand GetCommand(DependencyObject obj)
		{
			return (ICommand)obj.GetValue(CommandProperty);
		}

		public static void SetCommand(DependencyObject obj, ICommand value)
		{
			obj.SetValue(CommandProperty, value);
		}

		private static void CommandPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			var button = sender as ButtonBase;
			if (button.IsNotNull())
			{
				if (e.OldValue != null)
				{
					ExecuteCommandBehavior.TryDetach(button);
				}
				if (e.NewValue != null)
				{					
					ExecuteCommandBehavior.Attach(button);
				}
			}
		}		

		public static readonly DependencyProperty ParameterProperty =
			DependencyProperty.RegisterAttached("Parameter", typeof(object),
			typeof(Commanding), null);

		public static object GetParameter(DependencyObject obj)
		{
			return (object)obj.GetValue(ParameterProperty);
		}

		public static void SetParameter(DependencyObject obj, object value)
		{
			obj.SetValue(ParameterProperty, value);
		}
	}
}