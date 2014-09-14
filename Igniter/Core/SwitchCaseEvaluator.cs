using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;

namespace Igniter.Core
{
    internal class SwitchCaseEvaluator : ISupportInitialize
    {
        private readonly DependencyObject _host;

        private readonly IEnumerable<DependencyObject> _hostCases;

        private static readonly object NotCompatible = new object();
        private static readonly object NotDetermined = new object();

        private DependencyObject[] _cases;
        private object[] _convertedValues;
        private IEqualityComparer _effectivEqualityComparer = NullEqualityComparer.Instance;
        private Type _targetType;
        private TypeConverter _typeConverter;

        private int _activeCaseIndex = int.MaxValue;

        public event EventHandler<SwitchCaseEventArgs> CaseSelected = delegate { };

        public SwitchCaseEvaluator(DependencyObject host, IEnumerable<DependencyObject> cases)
        {
            _host = host;
            _hostCases = cases;
        }

        private void OnCaseSelected(DependencyObject @case)
        {
            CaseSelected(this, new SwitchCaseEventArgs(@case));
        }

        public void BeginInit() {}

        public void EndInit()
        {
            _cases = _hostCases.ToArray();

            _convertedValues = new object[_cases.Length];
            
            SetEvaluatorOnHost(_host, this);

            SetTargetType(GetOn(_host));
            UpdateEffectiveEqualityComparer();

            for (int i = 0; i < _cases.Length; i++)
            {
                SetEvaluatorOnCase(_cases[i], this);
                SetCaseIndex(_cases[i], i);
            }

            ChooseCase(clearCache: true);
        }

        #region object On { get; set; }

        private object On { get { return GetOn(_host); } }

        /// <summary>
        /// Identifies the <see cref="On"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OnProperty =
            DependencyProperty.RegisterAttached("On", typeof(object), typeof(SwitchCaseEvaluator), new PropertyMetadata(OnOnChanged));

        private static void OnOnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var evaluator = GetEvaluatorOnHost(d);

            if (evaluator != null)
                evaluator.SetTargetType(e.NewValue);
        }

        /// <summary>
        /// Gets the On.
        /// </summary>
        public static object GetOn(DependencyObject obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            return (object)obj.GetValue(OnProperty);
        }

        /// <summary>
        /// Sets the On.
        /// </summary>
        public static void SetOn(DependencyObject obj, object value)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            obj.SetValue(OnProperty, value);
        }

        #endregion

        private void SetTargetType(object switchOn)
        {
            var newType = switchOn != null ? switchOn.GetType() : null;

            var previousType = _targetType;
            _targetType = newType;

            var typeChanged = previousType != _targetType;

            if (typeChanged)
            {
                _typeConverter = newType == null ? null : TypeDescriptor.GetConverter(_targetType);
                UpdateEffectiveEqualityComparer();
            }

            ChooseCase(clearCache: typeChanged);
        }

        private void UpdateEffectiveEqualityComparer()
        {
            _effectivEqualityComparer = EqualityComparer ?? GetDefaultEqualityComparer(_targetType);
        }

        #region object When { get; set; }

        /// <summary>
        /// Identifies the <see cref="When"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WhenProperty =
            DependencyProperty.RegisterAttached("When", typeof(object), typeof(SwitchCaseEvaluator), new PropertyMetadata(OnWhenPropertyChanged));

        private static void OnWhenPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var evaluator = GetEvaluatorOnCase(sender);

            if (evaluator != null)
                evaluator.OnCaseValueChanged(GetCaseIndex(sender));
        }

        /// <summary>
        /// Gets the When.
        /// </summary>
        public object GetWhen(DependencyObject obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            return (object)obj.GetValue(WhenProperty);
        }

        /// <summary>
        /// Sets the When.
        /// </summary>
        public void SetWhen(DependencyObject obj, object value)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            obj.SetValue(WhenProperty, value);
        }

        #endregion

        #region int CaseIndex { get; private set; }

        private static readonly DependencyPropertyKey CaseIndexPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("CaseIndex", typeof(int), typeof(SwitchCaseEvaluator), new PropertyMetadata(-1));

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

        #region SwitchCaseEvaluator EvaluatorOnHost { get; private set; }

        private static readonly DependencyPropertyKey EvaluatorOnHostPropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("EvaluatorOnHost", typeof(SwitchCaseEvaluator), typeof(SwitchCaseEvaluator), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="EvaluatorOnHost"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EvaluatorOnHostProperty = EvaluatorOnHostPropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the EvaluatorOnHost.
        /// </summary>
        public static SwitchCaseEvaluator GetEvaluatorOnHost(DependencyObject obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            return (SwitchCaseEvaluator)obj.GetValue(EvaluatorOnHostProperty);
        }

        private static void SetEvaluatorOnHost(DependencyObject obj, SwitchCaseEvaluator value)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            obj.SetValue(EvaluatorOnHostPropertyKey, value);
        }

        #endregion

        #region SwitchCaseEvaluator EvaluatorOnCase { get; private set; }

        private static readonly DependencyPropertyKey EvaluatorOnCasePropertyKey =
            DependencyProperty.RegisterAttachedReadOnly("EvaluatorOnCase", typeof(SwitchCaseEvaluator), typeof(SwitchCaseEvaluator), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="EvaluatorOnCase"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EvaluatorOnCaseProperty = EvaluatorOnCasePropertyKey.DependencyProperty;

        /// <summary>
        /// Gets the EvaluatorOnCase.
        /// </summary>
        public static SwitchCaseEvaluator GetEvaluatorOnCase(DependencyObject obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            return (SwitchCaseEvaluator)obj.GetValue(EvaluatorOnCaseProperty);
        }

        private static void SetEvaluatorOnCase(DependencyObject obj, SwitchCaseEvaluator value)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            obj.SetValue(EvaluatorOnCasePropertyKey, value);
        }

        #endregion

        #region IEqualityComparer EqualityComparer { get; set; }

        private IEqualityComparer EqualityComparer
        {
            get { return GetEqualityComparer(_host); }
        }

        /// <summary>
        /// Identifies the <see cref="EqualityComparer"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EqualityComparerProperty =
            DependencyProperty.RegisterAttached("EqualityComparer", typeof(IEqualityComparer), typeof(SwitchCaseEvaluator),
            new PropertyMetadata(OnEqualityComparerPropertyChanged));

        private static void OnEqualityComparerPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var evaluator = GetEvaluatorOnHost(sender);
            
            if (evaluator != null)
            {
                evaluator.UpdateEffectiveEqualityComparer();
                evaluator.ChooseCase();
            }
        }

        /// <summary>
        /// Gets the EqualityComparer.
        /// </summary>
        public static IEqualityComparer GetEqualityComparer(DependencyObject obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            return (IEqualityComparer)obj.GetValue(EqualityComparerProperty);
        }

        /// <summary>
        /// Sets the EqualityComparer.
        /// </summary>
        public static void SetEqualityComparer(DependencyObject obj, IEqualityComparer value)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            obj.SetValue(EqualityComparerProperty, value);
        }

        #endregion

        private IEqualityComparer GetDefaultEqualityComparer(Type type)
        {
            if (type == null) return NullEqualityComparer.Instance;

            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.GetProperty;

            return (IEqualityComparer)typeof(EqualityComparer<>).MakeGenericType(type).InvokeMember("Default", flags, null, null, null);
        }

        private bool EvaluateCase(int caseIndex)
        {
            if (caseIndex > _activeCaseIndex)
                throw new InvalidOperationException("Cannot evaluate lower priority case");

            var @case = _cases[caseIndex];

            if (@case.IsPropertySet(WhenProperty))
            {
                var convertedValue = _convertedValues[caseIndex];

                if (convertedValue == NotDetermined)
                {
                    var value = TryConvert(GetWhen(_cases[caseIndex]));
                    convertedValue = _convertedValues[caseIndex] = TryConvert(value);
                }

                if (convertedValue == NotCompatible ||
                    !_effectivEqualityComparer.Equals(On, convertedValue))
                    return false;
            }
            else if (caseIndex < _cases.Length - 1)
                return false; // only the last case may have no When
            // else case is default

            _activeCaseIndex = caseIndex;
            OnCaseSelected(_cases[caseIndex]);
            return true;
        }

        private void ChooseCase(bool clearCache = false, int startIndex = 0)
        {
            _activeCaseIndex = int.MaxValue;

            int i;
            bool found = false;
            for (i = startIndex; i < _cases.Length; i++)
            {
                if (clearCache)
                    _convertedValues[i] = NotDetermined;

                if (found = EvaluateCase(i))
                    break;
            }

            if (clearCache)
                for (; i < _cases.Length; i++)
                    _convertedValues[i] = NotDetermined;

            if (!found)
            {
                _activeCaseIndex = int.MaxValue;
                OnCaseSelected(null);
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
            if (_targetType == null || _targetType.IsInstanceOfType(value))
                return value;
            
            try
            {
                return _typeConverter.ConvertFrom(value);
            }
            catch { }

            return NotCompatible;
        }

        private class NullEqualityComparer : IEqualityComparer
        {
            private NullEqualityComparer() { }

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

    internal class SwitchCaseEventArgs : EventArgs 
    {
        public SwitchCaseEventArgs(DependencyObject @case)
        {
            Case = @case;
        }

        public DependencyObject Case { get; private set; }
    }
}