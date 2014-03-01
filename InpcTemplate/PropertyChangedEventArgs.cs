using System.ComponentModel;

namespace NorthHorizon.Samples.InpcTemplate
{
    public class PropertyChangedEventArgs<T> : PropertyChangedEventArgs
    {
        public PropertyChangedEventArgs(string propertyName, T oldValue, T newValue)
            : base(propertyName)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }

        public T OldValue { get; set; }
        public T NewValue { get; set; }
    }
}
