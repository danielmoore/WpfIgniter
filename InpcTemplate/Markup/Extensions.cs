using System;
using System.IO.Packaging;
using System.Windows.Markup;

namespace NorthHorizon.Samples.InpcTemplate.Markup
{
    public static class Extensions
    {
        public static T GetService<T>(this IServiceProvider serviceProvider)
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");

            return (T)serviceProvider.GetService(typeof(T));
        }

        public static Uri ResolvePartUri(this IUriContext uriContext, Uri uri)
        {
            if (uriContext == null) throw new ArgumentNullException("uriContext");
            if (uri == null) throw new ArgumentNullException("uri");

            if (uri.IsAbsoluteUri) return PackUriHelper.GetPartUri(uri);

            return PackUriHelper.ResolvePartUri(PackUriHelper.GetPartUri(uriContext.BaseUri), uri);
        }
    }
}
