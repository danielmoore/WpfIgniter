using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Windows;
using System.Windows.Markup;

namespace NorthHorizon.Samples.InpcTemplate.Markup
{
    /// <summary>
    /// Provides cached resource dictionaries to avoid loading the same resources multiple times.
    /// </summary>
    public sealed class SharedResourceDisctionaryExtension : MarkupExtension
    {
        private static readonly Dictionary<Uri, ResourceDictionary> CachedDictionaries = new Dictionary<Uri, ResourceDictionary>(new PartUriEqualityComparer());

        /// <summary>
        /// Gets or sets the URI of the dictionary to load.
        /// </summary>
        public Uri Source { get; set; }

        /// <summary>
        /// When implemented in a derived class, returns an object that is provided as the value of the target property for this markup extension.
        /// </summary>
        /// <param name="serviceProvider">A service provider helper that can provide services for the markup extension.</param>
        /// <returns>
        /// The object value to set on the property where the extension is applied.
        /// </returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Source == null) return null;

            var uri = serviceProvider.GetService<IUriContext>().ResolvePartUri(Source);

            ResourceDictionary dictionary;
            if (!CachedDictionaries.TryGetValue(uri, out dictionary))
                CachedDictionaries.Add(uri, dictionary = new ResourceDictionary { Source = Source });

            return dictionary;
        }

        private class PartUriEqualityComparer : IEqualityComparer<Uri>
        {
            public bool Equals(Uri x, Uri y)
            {
                return PackUriHelper.ComparePartUri(x, y) == 0;
            }

            public int GetHashCode(Uri obj)
            {
                return PackUriHelper.GetNormalizedPartUri(obj).GetHashCode();
            }
        }
    }
}
