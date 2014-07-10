using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Markup;

namespace Igniter.Behaviors
{
    [ContentProperty("Setters")]
    public class SwitchMultiPropertyCase : SwitchBehaviorCase, IAddChild
    {
        public SwitchMultiPropertyCase()
        {
            Setters = new List<PropertySetter>();
        }

        public List<PropertySetter> Setters { get; private set; }

        public override void ApplyState(DependencyObject obj)
        {
            foreach (var setter in Setters)
                SetPropertyHelper.SetBindingOrValue(obj, setter.Property, setter.Value);
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
}