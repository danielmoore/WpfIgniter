using System;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;
using System.Windows.Media;
using System.Windows.Threading;

namespace Igniter.Behaviors
{
    public class ItemPositionBehavior : Behavior<ItemsControl>
    {
        #region bool IsEnabled { get; set; }

        /// <summary>
        /// Identifies the <see cref="IsEnabled"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(ItemPositionBehavior),
                new PropertyMetadata(OnIsEnabledPropertyChanged));

        /// <summary>
        /// Gets the IsEnabled.
        /// </summary>
        public bool GetIsEnabled(DependencyObject obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            return (bool)obj.GetValue(IsEnabledProperty);
        }

        /// <summary>
        /// Sets the IsEnabled.
        /// </summary>
        public void SetIsEnabled(DependencyObject obj, bool value)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            obj.SetValue(IsEnabledProperty, value);
        }

        private static void OnIsEnabledPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var behaviors = Interaction.GetBehaviors(sender);

            int existingIdx = -1;

            for (var i = 0; i < behaviors.Count; i++)
                if (behaviors[i] is ItemPositionBehavior)
                {
                    existingIdx = i;
                    break;
                }

            if ((bool)e.NewValue)
            {
                if (existingIdx < 0)
                    behaviors.Add(new ItemPositionBehavior());
            }
            else if (existingIdx >= 0)
                behaviors.RemoveAt(existingIdx);
        }

        #endregion

        #region ItemPositionManager ItemPositionManager { get; private set; }

        private static readonly DependencyPropertyKey ItemPositionManagerPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("ItemPositionManager",
                typeof(ItemPositionManager), typeof(ItemPositionBehavior), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="ItemPositionManager"/> dependency property.
        /// </summary>
        private static readonly DependencyProperty ItemPositionManagerProperty = ItemPositionManagerPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the ItemPositionManager.
        /// </summary>
        private static ItemPositionManager GetItemPositionManager(DependencyObject obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            return (ItemPositionManager)obj.GetValue(ItemPositionManagerProperty);
        }

        private static void SetItemPositionManager(DependencyObject obj, ItemPositionManager value)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            obj.SetValue(ItemPositionManagerPropertyKey, value);
        }

        #endregion

        #region bool IsFirst { get; private set; }

        private static readonly DependencyPropertyKey IsFirstPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("IsFirst", typeof(bool), typeof(ItemPositionBehavior), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="IsFirst"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsFirstProperty = IsFirstPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the IsFirst.
        /// </summary>
        public static bool GetIsFirst(DependencyObject obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            return (bool)obj.GetValue(IsFirstProperty);
        }

        private static void SetIsFirst(DependencyObject obj, bool value)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            obj.SetValue(IsFirstPropertyKey, value);
        }

        #endregion

        #region bool IsLast { get; private set; }

        private static readonly DependencyPropertyKey IsLastPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("IsLast", typeof(bool), typeof(ItemPositionBehavior), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="IsLast"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsLastProperty = IsLastPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the IsLast.
        /// </summary>
        public static bool GetIsLast(DependencyObject obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            return (bool)obj.GetValue(IsLastProperty);
        }

        private static void SetIsLast(DependencyObject obj, bool value)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            obj.SetValue(IsLastPropertyKey, value);
        }

        #endregion

        protected override void OnAttached()
        {
            base.OnAttached();

            if (GetIsEnabled(AssociatedObject))
            {
                Interaction.GetBehaviors(AssociatedObject).Remove(this);
                return;
            }

            SetIsEnabled(AssociatedObject, true);

            if (AssociatedObject.IsLoaded)
                SetupPositionManagers(AssociatedObject, AssociatedObject);
            else
                AssociatedObject.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            AssociatedObject.Loaded -= OnLoaded;

            SetupPositionManagers(AssociatedObject, AssociatedObject);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            var behaviors = Interaction.GetBehaviors(AssociatedObject);

            SetIsEnabled(AssociatedObject, behaviors.Any(b => !ReferenceEquals(b, this) && b is ItemPositionBehavior));
        }

        private static void SetupPositionManagers(ItemsControl host, Visual target)
        {
            var frameworkElement = target as FrameworkElement;
            if (frameworkElement != null)
                frameworkElement.ApplyTemplate();

            var count = VisualTreeHelper.GetChildrenCount(target);

            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(target, i) as Visual;

                if (child == null) continue;

                var panel = child as Panel;
                if (panel != null && panel.IsItemsHost && GetItemPositionManager(panel) == null)
                {
                    var gen = GetItemContainerGeneratorForPanel(host, panel);
                    if (gen != null)
                    {
                        var mgr = new ItemPositionManager(host, panel, gen);

                        SetItemPositionManager(panel, mgr);

                        mgr.Refresh();

                        // Refresh will call SetupPositionManagers() as appropriate.
                        continue;
                    }
                }

                SetupPositionManagers(host, child);
            }
        }

        private static ItemContainerGenerator GetItemContainerGeneratorForPanel(ItemsControl host, Panel panel)
        {
            return ((IItemContainerGenerator)host.ItemContainerGenerator).GetItemContainerGeneratorForPanel(panel);
        }

        private class ItemPositionManager
        {
            private readonly ItemsControl _host;
            private readonly Panel _panel;
            private readonly ItemContainerGenerator _generator;

            private WeakReference _firstRef, _lastRef;

            public ItemPositionManager(ItemsControl host, Panel panel, ItemContainerGenerator generator)
            {
                _host = host;
                _panel = panel;
                _generator = generator;

                _generator.ItemsChanged += OnItemsChanged;
            }

            public void Refresh()
            {
                var children = _panel.Children;
                if (children.Count == 0)
                    ClearFirstAndLast();
                else
                {
                    UpdateFirst();
                    UpdateLast();

                    SetupPositionManagers(_host, _panel);
                }
            }

            private void ClearFirstAndLast()
            {
                ExchangeItem(IsFirstPropertyKey, ref _firstRef, null);
                ExchangeItem(IsLastPropertyKey, ref _lastRef, null);
            }

            private void OnItemsChanged(object sender, ItemsChangedEventArgs e)
            {
                // Dispatch this so that _panel has an opportunity to (synchronously) 
                // make items on the current event dispatch.
                _host.Dispatcher.BeginInvoke(() => HandleItemsChange(e), DispatcherPriority.Send);
            }

            private void HandleItemsChange(ItemsChangedEventArgs e)
            {
                var count = e.ItemCount;
                var generator = (IItemContainerGenerator)_generator;

                var idx = generator.IndexFromGeneratorPosition(e.Position);
                var oldIdx = generator.IndexFromGeneratorPosition(e.OldPosition);

                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                    case NotifyCollectionChangedAction.Replace:
                        if (idx == 0)
                            UpdateFirst();

                        if (idx + e.ItemCount >= count)
                            UpdateLast();

                        SetupPositionManagers(_host, _panel);                            
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        if (_panel.Children.Count > 0)
                        {
                            if (oldIdx == 0)
                                UpdateFirst();

                            if (oldIdx + e.ItemCount >= count)
                                UpdateLast();
                        }
                        else
                            ClearFirstAndLast();

                        break;
                    case NotifyCollectionChangedAction.Move:
                        if (idx == 0 || oldIdx == 0)
                            UpdateFirst();

                        if (idx + e.ItemCount >= count || oldIdx + e.ItemCount >= count)
                            UpdateLast();
                        break;
                    case NotifyCollectionChangedAction.Reset:
                        Refresh();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            private void UpdateFirst()
            {
                var item = _panel.Children[0];
                ExchangeItem(IsFirstPropertyKey, ref _firstRef, item);
            }

            private void UpdateLast()
            {
                var item = _panel.Children[_panel.Children.Count - 1];
                ExchangeItem(IsLastPropertyKey, ref _lastRef, item);
            }

            private static void ExchangeItem(DependencyPropertyKey key, ref WeakReference valueRef, DependencyObject newValue)
            {
                var target = valueRef != null ? (DependencyObject)valueRef.Target : null;

                if (ReferenceEquals(target, newValue)) return;

                if (target != null)
                    target.SetValue(key, false);

                if (newValue != null)
                {
                    valueRef = new WeakReference(newValue);
                    newValue.SetValue(key, true);
                }
                else
                    valueRef = null;
            }
        }
    }
}