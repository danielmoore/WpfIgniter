using System.Windows.Input;

namespace NorthHorizon.Samples.InpcTemplate.Tests.Live
{
    public class CommandParameterTestViewModel : BindableBase
    {
        public CommandParameterTestViewModel()
        {
            TestCommand = new ExpressionCommand<int>(v => { }, v => v > Value1);
        }

        public ICommand TestCommand { get; private set; }

        private int _value1;
        public int Value1
        {
            get { return _value1; }
            set { SetProperty(ref _value1, value); }
        }
    }
}
