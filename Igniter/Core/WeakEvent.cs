using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Igniter.Core
{
    /// <summary>
    /// Manages a list of weak references to delegates.
    /// </summary>
    /// <typeparam name="TDelegate">The type of the delegate.</typeparam>
    public sealed class WeakEvent<TDelegate> where TDelegate : class
    {
        private readonly ConditionalWeakTable<object, List<TDelegate>> _delegates = new ConditionalWeakTable<object, List<TDelegate>>();
        private readonly List<WeakReference> _invocationLists = new List<WeakReference>();
        private readonly List<TDelegate> _staticInvocationList = new List<TDelegate>();

        private List<TDelegate> GetInvocationList(object target)
        {
            if (target == null) return _staticInvocationList;

            List<TDelegate> invocationList;
            if (!_delegates.TryGetValue(target, out invocationList))
            {
                _delegates.Add(target, invocationList = new List<TDelegate>(5));
                _invocationLists.Add(new WeakReference(invocationList));
            }

            return invocationList;
        }

        private static object GetTarget(TDelegate handler)
        {
            var @delegate = handler as Delegate;

            if (@delegate == null)
                throw new ArgumentException("Handler is not a delegate", "handler");

            if (@delegate.Target != null && Attribute.IsDefined(@delegate.Method.DeclaringType, typeof(CompilerGeneratedAttribute)))
                throw new ArgumentException("Handler is compiler generated and cannot be added to a weak event", "handler");

            return @delegate.Target;
        }

        /// <summary>
        /// Adds the specified event handler to the invocation list.
        /// </summary>
        /// <param name="handler">The handler to add.</param>
        [DebuggerStepThrough]
        public void Add(TDelegate handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");

            var target = GetTarget(handler);

            GetInvocationList(target).Add(handler);
        }

        /// <summary>
        /// Removes the specified event handler from the invocation list.
        /// </summary>
        /// <param name="handler">The handler to remove.</param>
        [DebuggerStepThrough]
        public void Remove(TDelegate handler)
        {
            if (handler == null) throw new ArgumentNullException("handler");

            var target = GetTarget(handler);

            GetInvocationList(target).Remove(handler);
        }

        /// <summary>
        /// Invokes each handler in the invocation list with the specified invoker.
        /// </summary>
        /// <param name="invoker">An action that, when called, invokes the given handler with the proper arguments.</param>
        [DebuggerStepThrough]
        public void Invoke(Action<TDelegate> invoker)
        {
            if (invoker == null) throw new ArgumentNullException("invoker");

            var removed = 0;

            var count = _invocationLists.Count;
            for (int i = 0; i < count; i++)
            {
                var invocationList = (List<TDelegate>)_invocationLists[i].Target;

                if (invocationList == null)
                    removed++;
                else
                {
                    for (var j = 0; j < invocationList.Count; j++)
                        invoker(invocationList[j]);

                    _invocationLists[i - removed] = _invocationLists[i];
                }
            }

            if (removed > 0)
                _invocationLists.RemoveRange(count - removed, removed);
            
            int staticCount = _staticInvocationList.Count;
            for (int i = 0; i < staticCount; i++)
                invoker(_staticInvocationList[i]);
        }
    }
}