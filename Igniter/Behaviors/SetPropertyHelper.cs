using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Igniter.Behaviors
{
    internal static class SetPropertyHelper
    {
        public static bool TryReceiveMarkupExtension(MarkupExtension markupExtension, IServiceProvider serviceProvider, out object value)
        {
            var staticResource = markupExtension as StaticResourceExtension;
            if (staticResource != null)
            {
                value = staticResource.ProvideValue(serviceProvider);
                return true;
            }

            if (markupExtension is DynamicResourceExtension || markupExtension is BindingBase)
            {
                value = markupExtension;
                return true;
            }

            value = null;
            return false;
        }

        public static void SetBindingOrValue(DependencyObject target, DependencyProperty property, object value)
        {
            var binding = value as BindingBase;

            if (binding != null)
            {
                BindingOperations.SetBinding(target, property, binding);
                return;
            }

            var markupExtension = value as MarkupExtension;
            if (markupExtension != null)
            {
                var computed = markupExtension.ProvideValue(new ServiceProvider { TargetObject = target, TargetProperty = property });

                target.SetValue(property, computed);
                return;
            }


            if (value is Expression || property.PropertyType.IsInstanceOfType(value))
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

        private class ServiceProvider : IServiceProvider, IProvideValueTarget
        {
            public object GetService(Type serviceType)
            {
                if (serviceType.IsInstanceOfType(this))
                    return this;

                throw new NotSupportedException();
            }

            public object TargetObject { get; set; }
            public object TargetProperty { get; set; }
        }

    }
}