using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using NUnit.Framework;

namespace Igniter.Tests
{
    [TestFixture]
    public class ExpressionCommandTestFixture
    {
        [Test]
        public void TestSimpleCommand()
        {
            var sut = new TestViewModel();

            int updateCount = 0;
            sut.SimpleCommand.CanExecuteChanged += (s, e) => updateCount++;

            sut.Value1 = 3;

            updateCount.ShouldEqual(1);
        }

        [Test]
        public void TestComplexCommand()
        {
            var sut = new TestViewModel
            {
                Value2 = new TestSubViewModel()
            };

            int updateCount = 0;
            sut.ComplexCommand.CanExecuteChanged += (s, e) => updateCount++;

            sut.Value2 = new TestSubViewModel();

            updateCount.ShouldEqual(1);

            sut.Value2.Value5 = new TestSubViewModel();

            updateCount.ShouldEqual(2);

            sut.Value2.Value5.Value4 = 3;

            updateCount.ShouldEqual(3);
        }

        [Test]
        public void TestComplexCollectionCommand()
        {
            var sut = new TestViewModel
            {
                Value2 = new TestSubViewModel()
            };

            int updateCount = 0;
            sut.ComplexCollectionCommand.CanExecuteChanged += (s, e) => updateCount++;

            sut.Value2 = new TestSubViewModel();

            updateCount.ShouldEqual(1);

            sut.Value2.Value6.Add(sut.Value2);

            updateCount.ShouldEqual(2);
        }

        [Test]
        public void TestComplexDepenencyObjectCommand()
        {
            var sut = new TestViewModel
            {
                Value3 = new TestDependencyObjectViewModel()
            };

            int updateCount = 0;
            sut.ComplexDependencyObjectCommand.CanExecuteChanged += (s, e) => updateCount++;

            sut.Value3 = new TestDependencyObjectViewModel();

            updateCount.ShouldEqual(1);

            sut.Value3.Value7 = 3;

            updateCount.ShouldEqual(2);
        }

        private class TestViewModel : BindableBase
        {
            public TestViewModel()
            {
                SimpleCommand = new ExpressionCommand(OnExecute, () => Value1 > 5);

                ComplexCommand = new ExpressionCommand(OnExecute, () => Value2.Value5.Value4 > 5);

                ComplexCollectionCommand = new ExpressionCommand(OnExecute, () => Value2.Value6.Contains(Value2));

                ComplexDependencyObjectCommand = new ExpressionCommand(OnExecute, () => Value3.Value7 > 5);
            }

            public ICommand SimpleCommand { get; private set; }

            public ICommand ComplexCommand { get; private set; }

            public ICommand ComplexCollectionCommand { get; private set; }

            public ICommand ComplexDependencyObjectCommand { get; private set; }

            private int _value1;
            public int Value1
            {
                get { return _value1; }
                set { SetProperty(ref _value1, value); }
            }

            private TestSubViewModel _value2;
            public TestSubViewModel Value2
            {
                get { return _value2; }
                set { SetProperty(ref _value2, value); }
            }

            private TestDependencyObjectViewModel _value3;
            public TestDependencyObjectViewModel Value3
            {
                get { return _value3; }
                set { SetProperty(ref _value3, value); }
            }

            private void OnExecute() { }
        }

        private class TestSubViewModel : BindableBase
        {
            private int _value4;
            public int Value4
            {
                get { return _value4; }
                set { SetProperty(ref _value4, value); }
            }

            private TestSubViewModel _value5;
            public TestSubViewModel Value5
            {
                get { return _value5; }
                set { SetProperty(ref _value5, value); }
            }

            public ObservableCollection<TestSubViewModel> Value6 = new ObservableCollection<TestSubViewModel>();
        }

        private class TestDependencyObjectViewModel : DependencyObject
        {
            public int Value7
            {
                get { return (int)GetValue(Value7Property); }
                set { SetValue(Value7Property, value); }
            }

            // Using a DependencyProperty as the backing store for Value6.  This enables animation, styling, binding, etc...
            public static readonly DependencyProperty Value7Property =
                DependencyProperty.Register("Value7", typeof(int), typeof(TestDependencyObjectViewModel), new UIPropertyMetadata(0));
        }
    }
}
