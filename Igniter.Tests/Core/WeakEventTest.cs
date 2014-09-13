using System;
using System.Text;
using Igniter.Core;
using NUnit.Framework;

namespace Igniter.Tests.Core
{
    [TestFixture]
    public class WeakEventTest
    {
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotAddNonDelegate()
        {
            var ev = new WeakEvent<string>();

            ev.Add("foo");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void CannotAddClosure()
        {
            var ev = new WeakEvent<Action>();

            var builder = new StringBuilder();
            ev.Add(() => builder.Append("foobar"));
        }

        [Test]
        public void CanAddStaticLambda()
        {
            var ev = new WeakEvent<Action>();

            ev.Add(() => TestStaticMethod());
        }

        [Test]
        public void CanAddThisClosure()
        {
            var ev = new WeakEvent<Action>();

            ev.Add(() => TestInstanceMethod());
        }

        [Test]
        public void SubscribersCanBeCollected()
        {
            var ev = new WeakEvent<Action>();

            var sub = new Subscriber();
            var subRef = new WeakReference(sub);

            ev.Add(sub.Call);

            sub = null;

            GC.Collect();

            var callCount = 0;
            ev.Invoke(h => { 
                callCount++;
                h();
            });

            callCount.ShouldEqual(0);
            subRef.Target.Should(Be.Null);
        }

        [Test]
        public void SubscribersAreCalled()
        {
            var ev = new WeakEvent<Action>();

            var sub = new Subscriber();

            ev.Add(sub.Call);

            GC.Collect();

            var callCount = 0;
            ev.Invoke(h =>
            {
                callCount++;
                h();
            });

            callCount.ShouldEqual(1);
            sub.CallCount.ShouldEqual(1);
        }

        [Test]
        public void SubscribersCanBeRemoved()
        {
            var ev = new WeakEvent<Action>();

            var sub = new Subscriber();

            ev.Add(sub.Call);

            ev.Remove(sub.Call);

            GC.Collect();

            var callCount = 0;
            ev.Invoke(h =>
            {
                callCount++;
                h();
            });

            callCount.ShouldEqual(0);
            sub.CallCount.ShouldEqual(0);
        }
        
        private class Subscriber
        {
            public int CallCount { get; set; }

            public void Call()
            {
                CallCount++;
            }
        }

        private static int TestInstanceMethod()
        {
            return -1;
        }

        public static int TestStaticMethod()
        {
            return -1; 
        }
    }
}