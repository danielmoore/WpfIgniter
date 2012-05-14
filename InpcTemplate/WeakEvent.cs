using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NorthHorizon.Samples.InpcTemplate
{
    public class WeakEvent<TDelegate> where TDelegate : class
    {
        private readonly List<WeakReference> _delegates = new List<WeakReference>();

        [DebuggerStepThrough]
        public void Add(TDelegate handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");

            lock (_delegates)
                _delegates.Add(new WeakReference(handler));
        }

        [DebuggerStepThrough]
        public void Remove(TDelegate handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");

            lock (_delegates)
                for (int i = _delegates.Count - 1; i >= 0; i--)
                {
                    var current = _delegates[i].Target;
                    if (current == null)
                        _delegates.RemoveAt(i);
                    else if (current.Equals(handler))
                    {
                        _delegates.RemoveAt(i);
                        break;
                    }
                }
        }

        [DebuggerStepThrough]
        public void Invoke(Action<TDelegate> invoker)
        {
            if (invoker == null) throw new ArgumentNullException("invoker");

            lock (_delegates)
                for (int i = _delegates.Count - 1; i >= 0; i--)
                {
                    var current = _delegates[i].Target;
                    if (current == null)
                        _delegates.RemoveAt(i);
                    else
                        invoker((TDelegate)current);
                }
        }
    }
}
