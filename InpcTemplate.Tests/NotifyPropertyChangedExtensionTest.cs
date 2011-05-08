using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NorthHorizon.Samples.InpcTemplate.Tests
{
    [TestClass]
    public class NotifyPropertyChangedExtensionTest
    {
        [TestMethod]
        public void TestGetPropertyChanges()
        {
            var receivedValues = new List<string>();

            var testObj = new TestBindableType();

            using (testObj.GetPropertyChanges(t => t.MyValue).Subscribe(receivedValues.Add))
            {
                testObj.MyValue = "Foo";
                Assert.AreEqual(testObj.MyValue, receivedValues.Last());

                testObj.MyValue = "Bar";
                Assert.AreEqual(testObj.MyValue, receivedValues.Last());
            }

            testObj.MyValue = "Foo";
            Assert.AreEqual("Bar", receivedValues.Last());
        }

        private class TestBindableType : BindableBase
        {
            private string _myValue;
            public string MyValue
            {
                get { return _myValue; }
                set { SetProperty(ref _myValue, value, "MyValue"); }
            }
        }
    }
}
