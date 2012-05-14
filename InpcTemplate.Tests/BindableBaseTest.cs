using System;
using NUnit.Framework;

namespace NorthHorizon.Samples.InpcTemplate.Tests
{
    [TestFixture]
    public class BindableBaseTest
    {
        static TestBindableType _sut;

        [TestFixtureSetUp]
        public void FixureSetup()
        {
            _sut = new TestBindableType();
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void CheckForStackAnalysis()
        {
            _sut.MyBrokenValue = 4;
        }

        [Test]
        public void SupportsExplicitInterfaces()
        {
            ((ITestInterface)_sut).MyInterfaceValue = 5;
        }

        [Test]
        public void CanSubscribeToPropertyChanged()
        {
            bool propertyChanged = false;
            var subscription = _sut.SubscribeToPropertyChanged(m => m.MyValue, () => propertyChanged = true);
            _sut.MyValue = 4;
            Assert.IsTrue(propertyChanged);

            propertyChanged = false;

            subscription.Dispose();

            _sut.MyValue = 5;

            Assert.IsFalse(propertyChanged);
        }

        private class TestBindableType : BindableBase, ITestInterface
        {
            private int _myValue;
            public int MyValue
            {
                get { return _myValue; }
                set { SetProperty(ref _myValue, value, "MyValue"); }
            }

            private int _myBrokenValue;
            public int MyBrokenValue
            {
                get { return _myBrokenValue; }
                set { SetProperty(ref _myBrokenValue, value, "MyValue"); }
            }

            private int _myInterfaceValue;
            int ITestInterface.MyInterfaceValue
            {
                get { return _myInterfaceValue; }
                set { SetProperty(ref _myInterfaceValue, value, "MyInterfaceValue"); }
            }
        }

        private interface ITestInterface
        {
            int MyInterfaceValue { get; set; }
        }
    }
}
