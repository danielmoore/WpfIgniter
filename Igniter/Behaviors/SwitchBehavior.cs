using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Markup;
using Igniter.Core;

namespace Igniter.Behaviors
{
    [ContentProperty("Cases")]
    public class SwitchBehavior : Behavior<DependencyObject>, ISupportInitialize
    {
        private readonly SwitchCaseEvaluator _caseEvaluator;

        public SwitchBehavior()
        {
            Cases = new List<SwitchBehaviorCase>();
            _caseEvaluator = new SwitchCaseEvaluator(this, Cases);

            _caseEvaluator.CaseSelected += OnCaseSelected;
        }

        public List<SwitchBehaviorCase> Cases { get; private set; }

        #region object On { get; set; }

        /// <summary>
        /// Identifies the <see cref="On"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OnProperty =
            SwitchCaseEvaluator.OnProperty.AddOwner(typeof(SwitchBehavior));

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
            SwitchCaseEvaluator.EqualityComparerProperty.AddOwner(typeof(SwitchBehavior));

        /// <summary>
        /// Gets or sets the EqualityComparer.
        /// </summary>
        public IEqualityComparer EqualityComparer
        {
            get { return (IEqualityComparer)GetValue(EqualityComparerProperty); }
            set { SetValue(EqualityComparerProperty, value); }
        }

        #endregion

        private void OnCaseSelected(object sender, SwitchCaseEventArgs e)
        {
            if (e.Case != null)
                ((SwitchBehaviorCase)e.Case).ApplyState(AssociatedObject);
        }

        public void BeginInit()
        {
            _caseEvaluator.BeginInit();
        }

        public void EndInit()
        {
            foreach (var @case in Cases)
                @case.OwningSwitchBehavior = this;

            _caseEvaluator.EndInit();
        }

        public void AddChild(object value)
        { 
            if (value == null)
                throw new ArgumentNullException("value");

            var depObj = value as SwitchBehaviorCase;

            if (depObj == null)
                throw new ArgumentException("Not a SwitchBehaviorCase", "value");

            Cases.Add(depObj);
        }

        public void AddText(string text)
        {
            if (!string.IsNullOrEmpty(text))
                throw new NotSupportedException("Cannot add text to SwitchBehavior");
        }
    }
}