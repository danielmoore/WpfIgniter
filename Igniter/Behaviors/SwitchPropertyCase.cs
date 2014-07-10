using System;
using System.Windows;
using System.Windows.Markup;

namespace Igniter.Behaviors
{
    [XamlSetMarkupExtension("ReceiveMarkupExtension")]
    public class SwitchPropertyCase : SwitchBehaviorCase
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

            SetPropertyHelper.SetBindingOrValue(obj, GetProperty(propertySrc), Value);
        }

        public static void ReceiveMarkupExtension(object targetObject, XamlSetMarkupExtensionEventArgs eventArgs)
        {
            if (eventArgs.Member.Name != "Value") return;

            object value;
            eventArgs.Handled = SetPropertyHelper.TryReceiveMarkupExtension(eventArgs.MarkupExtension, eventArgs.ServiceProvider, out value);

            if (eventArgs.Handled)
                ((SwitchPropertyCase)targetObject).Value = value;
        }
    }
}