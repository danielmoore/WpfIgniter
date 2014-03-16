using System;
using System.IO.Packaging;
using System.Windows.Markup;

namespace Igniter
{
    /// <summary>
    /// Miscellaneous extension methods.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <typeparam name="TService">The type of the servie.</typeparam>
        /// <param name="serviceProvider">The service provider.</param>
        /// <returns>A service object of type <typeparamref name="TService"/>, or <c>null</c> if there is no service object of type <typeparamref name="TService"/>.</returns>
        /// <exception cref="System.ArgumentNullException">serviceProvider</exception>
        public static TService GetService<TService>(this IServiceProvider serviceProvider) where TService : class 
        {
            if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");

            return (TService)serviceProvider.GetService(typeof(TService));
        }

        /// <summary>
        /// Resolves a relative URI to an absolute part URI based on the <paramref name="uriContext"/>.
        /// </summary>
        /// <param name="uriContext">The URI context.</param>
        /// <param name="uri">The relative URI.</param>
        /// <returns>An absolute part URI.</returns>
        /// <exception cref="System.ArgumentNullException">uriContext, uri</exception>
        public static Uri ResolvePartUri(this IUriContext uriContext, Uri uri)
        {
            if (uriContext == null) throw new ArgumentNullException("uriContext");
            if (uri == null) throw new ArgumentNullException("uri");

            if (uri.IsAbsoluteUri) return PackUriHelper.GetPartUri(uri);

            return PackUriHelper.ResolvePartUri(PackUriHelper.GetPartUri(uriContext.BaseUri), uri);
        }
    }
}
