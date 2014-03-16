using System;
using System.Windows;
using System.Windows.Markup;
using Igniter.Core;

namespace Igniter.Behaviors
{
    /// <summary>
    /// Provides cached resource dictionaries to avoid loading the same resources multiple times.
    /// </summary>
    public sealed class SharedResourceBehavior : ResourceBehvior
    {
        /// <summary>
        /// Gets or sets the URI of the dictionary to load.
        /// </summary>
        public Uri Source { get; set; }

        protected override ResourceDictionary ProvideAttachedResources(IUriContext uriContext)
        {
            if (Source == null) return null;

            var uri = this.ResolvePartUri(Source);

            return ResourceCache.GetOrCreate(uri);
        }
    }
}