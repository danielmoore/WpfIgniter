using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Windows;
using System.Windows.Markup;

namespace NorthHorizon.Samples.InpcTemplate.Markup
{
    public class SharedResourceDisctionaryExtension : MarkupExtension
    {
        private static readonly Dictionary<Uri, ResourceDictionary> CachedDictionaries = new Dictionary<Uri, ResourceDictionary>(new PartUriEqualityComparer());

        public Uri Source { get; set; }

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
