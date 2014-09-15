using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Igniter.Tests
{
    [TestFixture]
    class ExpressionUtilTest
    {
        [Test]
        public void CanChooseSimpleProperty()
        {
            ExpressionUtil.GetPropertyName((BaseObject o) => o.PropA);
        }

        [Test]
        public void CanChooseInheritedProperty()
        {
            ExpressionUtil.GetPropertyName((DerivedObject o) => o.PropA);
        }

        [Test]
        public void CanChooseHidingdProperty()
        {
            ExpressionUtil.GetPropertyName((DerivedObject o) => o.PropC);
        }

        [Test]
        public void CanChooseOverriddendProperty()
        {
            ExpressionUtil.GetPropertyName((DerivedObject o) => o.PropB);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void CannotChooseClosedValue()
        {
            var variable = 3;
            ExpressionUtil.GetPropertyName((DerivedObject o) => variable);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void CannotChooseMethod()
        {
            var variable = 3;
            ExpressionUtil.GetPropertyName((DerivedObject o) => o.MethodA());
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void CannotChooseField()
        {
            var variable = 3;
            ExpressionUtil.GetPropertyName((DerivedObject o) => o.FieldA);
        }        

        private class BaseObject
        {
            public string PropA { get; set; }

            public virtual string PropB { get; set; }

            public string PropC { get; set; }

            public string FieldA;

            public int MethodA()
            {
                return -1;
            }
        }

        private class DerivedObject : BaseObject
        {
            public override string PropB { get; set; }
            public new string PropC { get; set; }
        }
    }
}
