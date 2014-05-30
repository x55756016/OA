namespace System.Windows.Input
{
	public class ActionCommand : ICommand
	{
		private Delegate _Delegate;
		private Delegate _CanExecute;
		public event EventHandler CanExecuteChanged;

		public ActionCommand(Action action)
			: this((Delegate)action, null)
		{
		}
		public ActionCommand(Action action, Func<bool> canExecute)
			: this((Delegate)action, canExecute)
		{
		}
		public ActionCommand(Action<object> action)
			: this((Delegate)action, null)
		{
		}
		public ActionCommand(Action<object> action, Func<object, bool> canExecute)
			: this((Delegate)action, canExecute)
		{
		}
		public ActionCommand(Action<object> action, Func<bool> canExecute)
			: this((Delegate)action, canExecute)
		{
		}
		private ActionCommand(Delegate action, Delegate canExecute)
		{
			this._Delegate = action;
			this._CanExecute = canExecute;
			this.OnCanExecuteChanged();
		}

		public void Execute(object parameter)
		{
			var del = this._Delegate;

			if (del.IsNull())
			{
				throw new NullReferenceException(
					"The underlying execute delegate wasn't setted.");
			}
			var length = del.Method.GetParameters().Length;

			if (length == 0)
			{
				del.DynamicInvoke();
			}
			else if (parameter.IsNull())
			{
				((Action<object>)del)(null);
				//throw new NullReferenceException(
				//    "The \"parameter\" is null and the underlying method expects a value.");
			}
			else
			{
				del.DynamicInvoke(parameter);
			}
		}

		public bool CanExecute(object parameter)
		{
			var del = this._CanExecute;

			if (del.IsNull())
			{
				return true;
			}
			var length = del.Method.GetParameters().Length;

			if (length == 0)
			{
				return (bool)del.DynamicInvoke();
			}
			else if (parameter.IsNull())
			{
				throw new NullReferenceException(
					"The \"parameter\" is null and the underlying method expects a value.");
			}
			else
			{
				return (bool)del.DynamicInvoke(parameter);
			}
		}

		public void OnCanExecuteChanged()
		{
			var handler = this.CanExecuteChanged;
			if (handler.IsNotNull())
			{
				handler(this, EventArgs.Empty);
			}
		}
	}
}