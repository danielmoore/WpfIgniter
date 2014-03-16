using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Windows;

namespace Igniter.Core
{
    internal static class ResourceCache
    {
        private static readonly Dictionary<Uri, WeakReference> CachedDictionaries = new Dictionary<Uri, WeakReference>(new PartUriEqualityComparer());

        public static ResourceDictionary GetOrCreate(Uri uri)
        {
            WeakReference resourceRef;
            ResourceDictionary resource = null;

            if (CachedDictionaries.TryGetValue(uri, out resourceRef))
                resource = (ResourceDictionary)resourceRef.Target;

            if (resource == null)
            {
                resource = new ResourceDictionary {Source = uri};
                CachedDictionaries[uri] = new WeakReference(resource);
            }

            return resource;
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