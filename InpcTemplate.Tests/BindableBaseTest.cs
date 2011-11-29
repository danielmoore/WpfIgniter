using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NorthHorizon.Samples.InpcTemplate.Tests
{
    [TestClass]
    public class BindableBaseTest
    {
        static TestBindableType _sut;

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            _sut = new TestBindableType();
        }
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        [TestMethod()]
        [ExpectedException(typeof(InvalidOperationException))]
        public void CheckForStackAnalysis()
        {
            _sut.MyBrokenValue = 4;
        }

        [TestMethod]
        public void SupportsExplicitInterfaces()
        {
            ((ITestInterface)_sut).MyInterfaceValue = 5;
        }

        [TestMethod]
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
