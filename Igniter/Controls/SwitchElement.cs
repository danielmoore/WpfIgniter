using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using Igniter.Core;

namespace Igniter.Controls
{
    [ContentProperty("Cases")]
    public class SwitchElement : FrameworkElement, IAddChild
    {
        private readonly SwitchCaseEvaluator _caseEvaluator;

        static SwitchElement()
        {
            var self = typeof(SwitchElement);
            FocusableProperty.OverrideMetadata(self, new FrameworkPropertyMetadata(false));
        }

        public SwitchElement()
        {
            Cases = new List<FrameworkElement>();

            _caseEvaluator = new SwitchCaseEvaluator(this, Cases);
            _caseEvaluator.CaseSelected += OnCaseSelected;
        }

        private void OnCaseSelected(object sender, SwitchCaseEventArgs e)
        {
            ResolvedView = (FrameworkElement)e.Case;
        }

        public override void BeginInit()
        {
            base.BeginInit();
            _caseEvaluator.BeginInit();
        }

        public override void EndInit()
        {
            base.EndInit();

            foreach (var @case in Cases)
                AddLogicalChild(@case);

            _caseEvaluator.EndInit();
        }

        #region FrameworkElement ResolvedView { get; private set; }

        private static readonly DependencyPropertyKey ResolvedViewPropertyKey =
            DependencyProperty.RegisterReadOnly("ResolvedView", typeof(FrameworkElement), typeof(SwitchElement),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, OnResolvedViewPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="ResolvedView"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ResolvedViewProperty = ResolvedViewPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the ResolvedView.
        /// </summary>
        public FrameworkElement ResolvedView
        {
            get { return (FrameworkElement)GetValue(ResolvedViewProperty); }
            private set { SetValue(ResolvedViewPropertyKey, value); }
        }

        private static void OnResolvedViewPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((SwitchElement)sender).OnResolvedViewPropertyChanged((FrameworkElement)e.OldValue, (FrameworkElement)e.NewValue);
        }

        private void OnResolvedViewPropertyChanged(FrameworkElement oldValue, FrameworkElement newValue)
        {
            if (oldValue != null)
            {
                RemoveVisualChild(oldValue);
            }

            if (newValue != null)
            {
                AddVisualChild(newValue);
            }
        }

        #endregion

        #region object On { get; set; }

        /// <summary>
        /// Identifies the <see cref="On"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OnProperty =
            SwitchCaseEvaluator.OnProperty.AddOwner(typeof(SwitchElement));

        /// <summary>
        /// Gets or sets the On.
        /// </summary>
        public object On
        {
            get { return (object)GetValue(OnProperty); }
            set { SetValue(OnProperty, value); }
        }

        #endregion

        #region IEqualityComparer EqualityComparer { get; set; }

        /// <summary>
        /// Identifies the <see cref="EqualityComparer"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EqualityComparerProperty =
            SwitchCaseEvaluator.EqualityComparerProperty.AddOwner(typeof(SwitchElement));

        /// <summary>
        /// Gets or sets the EqualityComparer.
        /// </summary>
        public IEqualityComparer EqualityComparer
        {
            get { return (IEqualityComparer)GetValue(EqualityComparerProperty); }
            set { SetValue(EqualityComparerProperty, value); }
        }

        #endregion

        #region object When { get; set; }

        /// <summary>
        /// Identifies the <see cref="When"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WhenProperty =
           SwitchCaseEvaluator.WhenProperty.AddOwner(typeof(SwitchElement));

        /// <summary>
        /// Gets the When.
        /// </summary>
        public static object GetWhen(DependencyObject obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            return (object)obj.GetValue(WhenProperty);
        }

        /// <summary>
        /// Sets the When.
        /// </summary>
        public static void SetWhen(DependencyObject obj, object value)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            obj.SetValue(WhenProperty, value);
        }

        #endregion

        public List<FrameworkElement> Cases { get; private set; }

        public void AddChild(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            var element = value as FrameworkElement;

            if (element == null)
                throw new ArgumentException("Not a FrameworkElement", "value");

            Cases.Add(element);
        }

        public void AddText(string text)
        {
            if (!string.IsNullOrEmpty(text))
                throw new NotSupportedException("Cannot add text to SwitchElement");
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var resolvedView = ResolvedView;

            if (resolvedView == null)
                return base.MeasureOverride(availableSize);

            resolvedView.Measure(availableSize);
            return resolvedView.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var resolvedView = ResolvedView;

            if (resolvedView == null)
                return base.ArrangeOverride(finalSize);

            resolvedView.Arrange(new Rect(finalSize));
            return resolvedView.RenderSize;
        }

        protected override int VisualChildrenCount
        {
            get { return ResolvedView != null ? 1 : 0; }
        }

        protected override Visual GetVisualChild(int index)
        {
            var resolvedView = ResolvedView;

            if (resolvedView == null || index > 0)
                throw new ArgumentOutOfRangeException();

            return resolvedView;
        }

        protected override IEnumerator LogicalChildren
        {
            get { return Cases.GetEnumerator(); }
        }
    }
}