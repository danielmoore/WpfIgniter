using System.Windows;
using System.Windows.Markup;

namespace Igniter.Behaviors
{
    [XamlSetMarkupExtension("ReceiveMarkupExtension")]
    public class PropertySetter
    {
        public DependencyProperty Property { get; set; }

        public object Value { get; set; }

        public static void ReceiveMarkupExtension(object targetObject, XamlSetMarkupExtensionEventArgs eventArgs)
        {
            if (eventArgs.Member.Name != "Value") return;

            object value;
            eventArgs.Handled = SetPropertyHelper.TryReceiveMarkupExtension(eventArgs.MarkupExtension, eventArgs.ServiceProvider, out value);

            if (eventArgs.Handled)
                ((PropertySetter)targetObject).Value = value;
        }
    }
}