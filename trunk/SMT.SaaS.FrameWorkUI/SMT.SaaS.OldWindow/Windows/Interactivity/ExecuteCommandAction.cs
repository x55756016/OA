using System.Windows.Input;

namespace System.Windows.Interactivity
{
	public class ExecuteCommandAction: TriggerAction<UIElement>
	{
		protected override void OnAttached()
		{
			base.OnAttached();
		}
		protected override void OnDetaching()
		{
			base.OnDetaching();
		}
		protected override void Invoke(object parameter)
		{
			var command = Commanding.GetCommand(this.AssociatedObject);
			command.Execute(null);
		}
	}
}