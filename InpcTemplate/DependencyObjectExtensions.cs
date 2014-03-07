using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Data;

namespace NorthHorizon.Samples.InpcTemplate
{
    public static class DependencyObjectExtensions
    {
        public static IDisposable SubscribeToDependencyPropertyChanges(this DependencyObject source, DependencyProperty property, DependencyPropertyChangedEventHandler handler)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (property == null) throw new ArgumentNullException("property");

            var proxies = GetEventProxies(source);

            if (proxies == null)
                SetEventProxies(source, proxies = new Dictionary<DependencyProperty, EventProxy>());

            EventProxy proxy;
            if (!proxies.TryGetValue(property, out proxy))
            {
                proxies.Add(property, proxy = new EventProxy(source));
                BindingOperations.SetBinding(proxy, EventProxy.ValueProperty, new Binding { Path = new PropertyPath("(0)", property), Source = source });
            }

            proxy.ValueChanged += handler;

            return Disposable.Create(() => proxy.ValueChanged -= handler);
        }

        public static IObservable<object> GetDependencyPropertyChanges(this DependencyObject source, DependencyProperty property)
        {
            return Observable.Create<object>(o => SubscribeToDependencyPropertyChanges(source, property, (s, e) => o.OnNext(e.NewValue)));
        }

        #region [Attached] private static Dictionary<DependencyProperty, EventProxy> EventProxies { get; set; }

        private static Dictionary<DependencyProperty, EventProxy> GetEventProxies(DependencyObject obj)
        {
            return (Dictionary<DependencyProperty, EventProxy>)obj.GetValue(EventProxiesProperty);
        }

        private static void SetEventProxies(DependencyObject obj, Dictionary<DependencyProperty, EventProxy> value)
        {
            obj.SetValue(EventProxiesProperty, value);
        }

        private static readonly DependencyProperty EventProxiesProperty =
            DependencyProperty.RegisterAttached("EventProxies", typeof(Dictionary<DependencyProperty, EventProxy>), typeof(DependencyObjectExtensions));

        #endregion

        private class EventProxy : DependencyObject
        {
            private readonly DependencyObject _target;

            public event DependencyPropertyChangedEventHandler ValueChanged = delegate { };

            public EventProxy(DependencyObject target)
            {
                _target = target;
            }

            #region public object Value { get; set; }

            public object Value
            {
                get { return GetValue(ValueProperty); }
                set { SetValue(ValueProperty, value); }
            }

            public static readonly DependencyProperty ValueProperty =
                DependencyProperty.Register("Value", typeof(object), typeof(EventProxy), new UIPropertyMetadata(OnValueChanged));

            private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
            {
                ((EventProxy)sender).OnValueChanged(e);
            }

            private void OnValueChanged(DependencyPropertyChangedEventArgs e)
            {
                ValueChanged(_target, e);
            }

            #endregion
        }
    }
}
