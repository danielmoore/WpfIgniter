using System;

namespace Igniter.Composition
{
    /// <summary>
    /// Options on how the <see cref="ViewFactory"/> should create a component.
    /// </summary>
    public enum CreationStrategy
    {
        /// <summary>
        /// The component will be provided as a parameter to the 
        /// <see cref="ViewFactory"/> method.
        /// </summary>
        Inject,
        /// <summary>
        /// The component will be resolved by the <see cref="IViewFactoryResolver"/> 
        /// and returned as an <code>out</code> parameter.
        /// </summary>
        Resolve,
        /// <summary>
        /// The component will be instantiated by <see cref="Activator"/>
        /// and returned as an <code>out</code> parameter.
        /// </summary>
        Activate
    }
}