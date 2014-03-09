using System;

namespace Igniter.Composition
{
    /// <summary>
    /// Provides a simple, common interface over any IoC container
    /// to resolve views and view models.
    /// </summary>
    public interface IViewFactoryResolver
    {
        /// <summary>
        /// Resolve the object by its given type.
        /// </summary>
        /// <param name="type">The type of the object to resolve.</param>
        /// <returns>The resolved object.</returns>
        object Resolve(Type type);

        /// <summary>
        /// Resolve the object by its given type.
        /// </summary>
        /// <typeparam name="T">The type of the object to resolve.</typeparam>
        /// <returns>The resolved object.</returns>
        T Resolve<T>();
    }
}