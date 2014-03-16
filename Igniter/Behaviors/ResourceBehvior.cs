using System;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Markup;

namespace Igniter.Behaviors
{
    public abstract class ResourceBehvior : Behavior<FrameworkElement>, IUriContext
    {
        private ResourceDictionary _attachedResources;

        Uri IUriContext.BaseUri { get; set; }

        protected abstract ResourceDictionary ProvideAttachedResources(IUriContext uriContext);

        protected override void OnAttached()
        {
            base.OnAttached();

            _attachedResources = ProvideAttachedResources(this);
            AssociatedObject.Resources.MergedDictionaries.Add(_attachedResources);
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Resources.MergedDictionaries.Remove(_attachedResources);
            _attachedResources = null;

            base.OnDetaching();
        }
    }
}