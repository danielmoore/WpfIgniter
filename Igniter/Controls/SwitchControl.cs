using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Igniter.Core;

namespace Igniter.Controls
{
    [ContentProperty("Cases")]
    public class SwitchControl : Control, IAddChild
    {
        private readonly SwitchCaseEvaluator _caseEvaluator;

        static SwitchControl()
        {
            var self = typeof(SwitchControl);
            var template = new ControlTemplate(self)
            {
                VisualTree = new FrameworkElementFactory(typeof(ContentPresenter))
            };

            TemplateProperty.OverrideMetadata(self, new FrameworkPropertyMetadata(template));
        }

        public SwitchControl()
        {
            Cases = new List<DependencyObject>();

            _caseEvaluator = new SwitchCaseEvaluator(this, Cases);
            _caseEvaluator.CaseSelected += OnCaseSelected;
        }

        private void OnCaseSelected(object sender, SwitchCaseEventArgs e)
        {
            Content = e.Case;
        }

        public override void BeginInit()
        {
            base.BeginInit();
            _caseEvaluator.BeginInit();
        }

        public override void EndInit()
        {
            base.EndInit();

           _caseEvaluator.EndInit();
        }

        #region object Content { get; private set; }

        private static readonly DependencyPropertyKey ContentPropertyKey =
            DependencyProperty.RegisterReadOnly("Content", typeof(object), typeof(SwitchControl), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="Content"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ContentProperty = ContentPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the Content.
        /// </summary>
        public object Content
        {
            get { return (object)GetValue(ContentProperty); }
            private set { SetValue(ContentPropertyKey, value); }
        }

        #endregion

        #region object On { get; set; }

        /// <summary>
        /// Identifies the <see cref="On"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OnProperty =
            SwitchCaseEvaluator.OnProperty.AddOwner(typeof(SwitchControl));

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
            SwitchCaseEvaluator.EqualityComparerProperty.AddOwner(typeof(SwitchControl));

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
           SwitchCaseEvaluator.WhenProperty.AddOwner(typeof(SwitchControl));

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

        public List<DependencyObject> Cases { get; private set; }

        public void AddChild(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            var depObj = value as DependencyObject;

            if (depObj == null)
                throw new ArgumentException("Not a dependency object", "value");

            Cases.Add(depObj);
        }

        public void AddText(string text)
        {
            if (!string.IsNullOrEmpty(text))
                throw new NotSupportedException("Cannot add text to SwitchControl");
        }
    }
}