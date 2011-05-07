using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;

namespace InpcTemplate
{
    /// <summary>
    /// Provides a basic implementation for <see cref="INotifyPropertyChanged"/> and <see cref="INotifyPropertyChanging"/>.
    /// </summary>
    public abstract class BindableBase : INotifyPropertyChanged, INotifyPropertyChanging
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        /// <summary>
        /// Occurs when a property value is changing.
        /// </summary>
        public event PropertyChangingEventHandler PropertyChanging = delegate { };

        /// <summary>
        /// Assigns the specified value to the specified backing store if a change has
        /// been made and, optionally, raises callbacks before and after.
        /// </summary>
        /// <typeparam name="T">The type of the property.</typeparam>
        /// <param name="backingStore">The backing store.</param>
        /// <param name="value">The new value.</param>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="onChanged">An optional callback to raise just before <see cref="PropertyChanged"/>.</param>
        /// <param name="onChanging">An optional callback to raise just before <see cref="PropertyChanging"/>.</param>
        /// <param name="coerceValue">An optional callback to coerce values before they are set.</param>
        protected void SetProperty<T>(ref T backingStore, T value, string propertyName, Action onChanged = null, Action<T> onChanging = null, Func<T, T> coerceValue = null)
        {
            VerifyCallerIsProperty(propertyName);

            var effectiveValue = coerceValue != null ? coerceValue(value) : value;

            if (EqualityComparer<T>.Default.Equals(backingStore, effectiveValue))
            {
                if (coerceValue != null && !ReferenceEquals(value, effectiveValue))
                    PropertyChangedEventManagerProxy.Instance.RaisePropertyChanged(this, propertyName);

                return;
            }

            if (onChanging != null) onChanging(effectiveValue);

            OnPropertyChanging(propertyName);

            backingStore = effectiveValue;

            if (onChanged != null) onChanged();

            OnPropertyChanged(propertyName);
        }

        [Conditional("DEBUG")]
        private void VerifyCallerIsProperty(string propertyName)
        {
            var stackTrace = new StackTrace();
            var frame = stackTrace.GetFrames()[2];
            var caller = frame.GetMethod();

            if (!caller.Name.Equals("set_" + propertyName, StringComparison.InvariantCulture))
                throw new InvalidOperationException(string.Format("Called SetProperty for {0} from {1}", propertyName, caller.Name));
        }

        /// <summary>
        /// Called when the given property has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Called when the given property is changing.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        protected virtual void OnPropertyChanging(string propertyName)
        {
            PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }

        private class PropertyChangedEventManagerProxy
        {
            private readonly NotifyPropertyChangedProxy _notifyPropertyChangedProxy;

            private PropertyChangedEventManagerProxy()
            {
                _notifyPropertyChangedProxy = new NotifyPropertyChangedProxy();
                PropertyChangedEventManager.AddListener(_notifyPropertyChangedProxy, new WeakListenerStub(), string.Empty);
            }

            public void RaisePropertyChanged(object sender, string propertyName)
            {
                _notifyPropertyChangedProxy.Raise(sender, new PropertyChangedEventArgs(propertyName));
            }

            private static PropertyChangedEventManagerProxy _instance;
            public static PropertyChangedEventManagerProxy Instance { get { return _instance ?? (_instance = new PropertyChangedEventManagerProxy()); } }

            private class NotifyPropertyChangedProxy : INotifyPropertyChanged
            {
                public event PropertyChangedEventHandler PropertyChanged = delegate { };

                public void Raise(object sender, PropertyChangedEventArgs e)
                {
                    PropertyChanged(sender, e);
                }
            }

            private class WeakListenerStub : IWeakEventListener
            {
                public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e) { return false; }
            }
        }
    }
}
