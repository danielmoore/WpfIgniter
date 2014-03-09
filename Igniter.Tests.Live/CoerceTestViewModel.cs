namespace Igniter.Tests.Live
{
    public class CoerceTestViewModel : BindableBase
    {
        private int _value;
        public int Value
        {
            get { return _value; }
            set { SetProperty(ref _value, value, coerceValue: CoerceValue); }
        }

        private int CoerceValue(int value)
        {
            return value < 0 ? _value : value;
        }
    }
}
