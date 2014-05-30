
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace System.Windows.Interactivity
{
	public class ExecuteCommandBehavior : Behavior<ButtonBase>
	{
		private ICommand _Command;
		private object _Parameter;

		protected override void OnAttached()
		{
			base.OnAttached();
			
			var button = this.AssociatedObject;
			var command = this._Command = Commanding.GetCommand(button);
			var parameter = this._Parameter = Commanding.GetParameter(button);
			this.ArrangeIsEnabled();
			command.CanExecuteChanged += this.Command_CanExecuteChanged;

			button.Click += this.Button_Click;

		}
		private void ArrangeIsEnabled()
		{
			this.AssociatedObject.IsEnabled = this._Command.CanExecute(this._Parameter);
		}
		private void Command_CanExecuteChanged(object sender, EventArgs e)
		{
			this.ArrangeIsEnabled();
		}
		private void Button_Click(object sender, RoutedEventArgs e)
		{
			this._Command.Execute(this._Parameter);
		}
		protected override void OnDetaching()
		{
			base.OnDetaching();
			this.AssociatedObject.Click -= this.Button_Click;
			this._Command.CanExecuteChanged -= this.Command_CanExecuteChanged;
		}

		public static void Attach(ButtonBase button)
		{
			var behavior = new ExecuteCommandBehavior();
			var behaviors = Interaction.GetBehaviors(button);
			behaviors.Add(behavior);			
		}

		public static void TryDetach(ButtonBase button)
		{
			var behaviors = Interaction.GetBehaviors(button);
			var behavior = behaviors.SingleOrDefault(b => b is ExecuteCommandBehavior);
			if (behavior.IsNotNull())
			{
				behaviors.Remove(behavior);
			}
		}
	}
}