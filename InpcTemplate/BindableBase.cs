using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;

namespace InpcTemplate
{
	public abstract class BindableBase : INotifyPropertyChanged, INotifyPropertyChanging
	{
		public event PropertyChangedEventHandler PropertyChanged = delegate { };
		public event PropertyChangingEventHandler PropertyChanging = delegate { };

		protected void SetProperty<T>(ref T backingStore, T value, string propertyName, Action onChanged = null, Action<T> onChanging = null)
		{
			VerifyCallerIsProperty(propertyName);

			if (EqualityComparer<T>.Default.Equals(backingStore, value)) return;

			if (onChanging != null) onChanging(value);

			OnPropertyChanging(propertyName);

			backingStore = value;

			if (onChanged != null) onChanged();

			OnPropertyChanged(propertyName);
		}

		[Conditional("DEBUG")]
		private void VerifyCallerIsProperty(string propertyName)
		{
			var stackTrace = new StackTrace();
			var frame = stackTrace.GetFrames()[2];
			var caller = frame.GetMethod();

			if (!caller.Name.Equals(propertyName, StringComparison.InvariantCulture))
				throw new InvalidOperationException(string.Format("Called SetProperty for {0} from {1}", propertyName, caller.Name));
		}

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		protected virtual void OnPropertyChanging(string propertyName)
		{
			PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
		}
	}
}
