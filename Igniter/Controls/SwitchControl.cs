using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;

namespace Igniter.Controls
{
    [ContentProperty("Cases")]
    public class SwitchControl : Control, IAddChild
    {
        private static readonly object NotCompatible = new object();
        private static readonly object NotDetermined = new object();

        private object[] _convertedValues;
        private IEqualityComparer _effectivEqualityComparer = NullEqualityComparer.Instance;
        private Type _targetType;

        private int _activeCaseIndex = int.MaxValue;

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
        }

        public override void EndInit()
        {
            base.EndInit();

            _convertedValues = new object[Cases.Count];

            for (int i = 0; i < Cases.Count; i++)
            {
                SetGoverningSwitchControl(Cases[i], this);
                SetCaseIndex(Cases[i], i);
            }

            

            ChooseCase(clearCache: true);
        }

        private static bool IsWhenSet(DependencyObject @case)
        {
            return @case.ReadLocalValue(WhenProperty) != DependencyProperty.UnsetValue;
        }

        private bool EvaluateCase(int caseIndex)
        {
            if (caseIndex > _activeCaseIndex)
                throw new InvalidOperationException("Cannot evaluate lower priority case");

            var @case = Cases[caseIndex];

            if (IsWhenSet(@case))
            {
                var convertedValue = _convertedValues[caseIndex];

                if (convertedValue == NotDetermined)
                {
                    var value = TryConvert(GetWhen(Cases[caseIndex]));
                    convertedValue = _convertedValues[caseIndex] = TryConvert(value);
                }

                if (convertedValue == NotCompatible ||
                    !_effectivEqualityComparer.Equals(Value, convertedValue))
                    return false;
            }
            else if (caseIndex < Cases.Count - 1)
                return false; // only the last case may have no When
            // else case is default

            _activeCaseIndex = caseIndex;
            Content = Cases[caseIndex];
            return true;
        }

        private void ChooseCase(bool clearCache = false, int startIndex = 0)
        {
            _activeCaseIndex = int.MaxValue;

            int i;
            bool found = false;
            for (i = startIndex; i < Cases.Count; i++)
            {
                if (clearCache)
                    _convertedValues[i] = NotDetermined;

                if (found = EvaluateCase(i))
                    break;
            }

            if (clearCache)
                for (; i < Cases.Count; i++)
                    _convertedValues[i] = NotDetermined;

            if (!found)
            {
                _activeCaseIndex = int.MaxValue;
                Content = null;
            }
        }

        private void OnCaseValueChanged(int caseIndex)
        {
            _convertedValues[caseIndex] = NotDetermined;

            // a higher priority case has already been selected
            if (_activeCaseIndex < caseIndex) return;

            // this is the current case and, if it becomes false, we should evaluate the next cases.
            if (_activeCaseIndex == caseIndex)
                ChooseCase(startIndex: caseIndex);

            // this is a higher priority case. If this becomes true, it takes precedence.
            if (_activeCaseIndex > caseIndex)
                EvaluateCase(caseIndex);
        }

        private object TryConvert(object value)
        {
            if (_targetType == null)
                return value;

            try
            {
                var strValue = value as string;

                if (_targetType.IsEnum && strValue != null)
                    return Enum.Parse(_targetType, strValue);

                if (typeof(IConvertible).IsAssignableFrom(_targetType) && value is IConvertible)
                    return Convert.ChangeType(value, _targetType);
            }
            catch {}

            return NotCompatible;
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

        #region object Value { get; set; }

        /// <summary>
        /// Identifies the <see cref="Value"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(SwitchControl), new PropertyMetadata(OnValuePropertyChanged));

        /// <summary>
        /// Gets or sets the Value.
        /// </summary>
        public object Value
        {
            get { return GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        private static void OnValuePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            ((SwitchControl)sender).OnValuePropertyChanged(args);
        }

        private void OnValuePropertyChanged(DependencyPropertyChangedEventArgs args)
        {
            var previousType = _targetType;
            _targetType = args.NewValue == null ? null : args.NewValue.GetType();

            var typeChanged = previousType != _targetType;

            if (typeChanged)
                _effectivEqualityComparer = EqualityComparer ?? GetDefaultEqualityComparer(_targetType);

            ChooseCase(clearCache: typeChanged);
        }

        #endregion

        #region IEqualityComparer EqualityComparer { get; set; }

        /// <summary>
        /// Identifies the <see cref="EqualityComparer"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EqualityComparerProperty =
            DependencyProperty.Register("EqualityComparer", typeof(IEqualityComparer), typeof(SwitchControl));

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
            DependencyProperty.RegisterAttached("When", typeof(object), typeof(SwitchControl), new PropertyMetadata(null, OnWhenPropertyChanged));

        /// <summary>
        /// Gets the When.
        /// </summary>
        public object GetWhen(DependencyObject obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            return obj.GetValue(WhenProperty);
        }

        /// <summary>
        /// Sets the When.
        /// </summary>
        public static void SetWhen(DependencyObject obj, object value)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            obj.SetValue(WhenProperty, value);
        }

        private static void OnWhenPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var switchControl = GetGoverningSwitchControl(sender);

            if (switchControl == null) return;

            switchControl.OnCaseValueChanged(GetCaseIndex(sender));
        }

        #endregion

        #region private SwitchControl GoverningSwitchControl { get; set; }

        private static readonly DependencyPropertyKey GoverningSwitchControlPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("GoverningSwitchControl", typeof(SwitchControl), typeof(SwitchControl), new PropertyMetadata(null));

        private static readonly DependencyProperty GoverningSwitchControlProperty = GoverningSwitchControlPropertyKey.DependencyProperty;

        private static SwitchControl GetGoverningSwitchControl(DependencyObject obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            return (SwitchControl)obj.GetValue(GoverningSwitchControlProperty);
        }

        private static void SetGoverningSwitchControl(DependencyObject obj, SwitchControl value)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            obj.SetValue(GoverningSwitchControlPropertyKey, value);
        }

        #endregion

        #region int CaseIndex { get; private set; }

        private static readonly DependencyPropertyKey CaseIndexPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("CaseIndex", typeof(int), typeof(SwitchControl), new PropertyMetadata(-1));

        private static readonly DependencyProperty CaseIndexProperty = CaseIndexPropertyKey.DependencyProperty;

        private static int GetCaseIndex(DependencyObject obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            return (int)obj.GetValue(CaseIndexProperty);
        }

        private static void SetCaseIndex(DependencyObject obj, int value)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            obj.SetValue(CaseIndexPropertyKey, value);
        }

        #endregion

        private IEqualityComparer GetDefaultEqualityComparer(Type type)
        {
            if (type == null) return NullEqualityComparer.Instance;

            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty;

            return (IEqualityComparer)typeof(EqualityComparer<>).MakeGenericType(type).InvokeMember("Default", flags, null, null, null);
        }

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

        private class NullEqualityComparer : IEqualityComparer
        {
            private NullEqualityComparer() {}

            bool IEqualityComparer.Equals(object x, object y)
            {
                return x == null && y == null || x != null && y != null;
            }

            public int GetHashCode(object obj)
            {
                throw new NotSupportedException();
            }

            public static readonly NullEqualityComparer Instance = new NullEqualityComparer();
        }
    }
}