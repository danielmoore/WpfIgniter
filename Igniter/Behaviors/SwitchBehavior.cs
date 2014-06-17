using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
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

    public abstract class SwitchBehaviorCase : DependencyObject
    {
        #region object When { get; set; }

        /// <summary>
        /// Identifies the <see cref="When"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WhenProperty =
            SwitchCaseEvaluator.WhenProperty.AddOwner(typeof(SwitchBehaviorCase));

        /// <summary>
        /// Gets or sets the When.
        /// </summary>
        public object When
        {
            get { return (object)GetValue(WhenProperty); }
            set { SetValue(WhenProperty, value); }
        }

        #endregion

        protected internal SwitchBehavior OwningSwitchBehavior { get; internal set; }

        public abstract void ApplyState(DependencyObject obj);
    }

    public abstract class SwitchPropertyCaseBase : SwitchBehaviorCase
    {
        protected static void SetBindingOrValue(DependencyObject target, DependencyProperty property, object value)
        {
            var binding = value as BindingBase;

            if (binding != null)
            {
                BindingOperations.SetBinding(target, property, binding);
                return;
            }


            if (property.PropertyType.IsInstanceOfType(value))
            {
                target.SetValue(property, value);
                return;
            }

            if (value != null)
            {
                var converter = TypeDescriptor.GetConverter(property.PropertyType);

                if (converter.CanConvertFrom(value.GetType()))
                {
                    var converted = converter.ConvertFrom(value);

                    target.SetValue(property, converted);

                    return;
                }
            }

            // if all else fails...
            target.ClearValue(property);
        }


    }

    public class SwitchPropertyCase : SwitchPropertyCaseBase
    {
        #region DependencyProperty Property { get; set; }

        /// <summary>
        /// Identifies the <see cref="Property"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PropertyProperty =
            DependencyProperty.RegisterAttached("Property", typeof(DependencyProperty), typeof(SwitchPropertyCase));

        /// <summary>
        /// Gets the Property.
        /// </summary>
        public static DependencyProperty GetProperty(DependencyObject obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            return (DependencyProperty)obj.GetValue(PropertyProperty);
        }

        /// <summary>
        /// Sets the Property.
        /// </summary>
        public static void SetProperty(DependencyObject obj, DependencyProperty value)
        {
            if (obj == null) throw new ArgumentNullException("obj");

            obj.SetValue(PropertyProperty, value);
        }

        #endregion

        public object Value { get; set; }

        public override void ApplyState(DependencyObject obj)
        {
            var propertySrc = this.IsPropertySet(PropertyProperty) ? (DependencyObject)this : OwningSwitchBehavior;
            SetBindingOrValue(obj, GetProperty(propertySrc), Value);
        }
    }

    [ContentProperty("Setters"), Ambient]
    public class SwitchMultiPropertyCase : SwitchPropertyCaseBase, IAddChild
    {
        public SwitchMultiPropertyCase()
        {
            Setters = new List<PropertySetter>();
        }

        public List<PropertySetter> Setters { get; private set; }

        public override void ApplyState(DependencyObject obj)
        {
            foreach (var setter in Setters)
                SetBindingOrValue(obj, setter.Property, setter.Value);
        }

        public void AddChild(object value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            var propSetter = value as PropertySetter;

            if (propSetter == null)
                throw new ArgumentException("Not a PropertySetter", "value");

            Setters.Add(propSetter);
        }

        public void AddText(string text)
        {
            if (!string.IsNullOrEmpty(text))
                throw new NotSupportedException("Cannot add text");
        }
    }

    public class PropertySetter
    {
        public DependencyProperty Property { get; set; }

        public object Value { get; set; }
    }
}