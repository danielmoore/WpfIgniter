using System;

namespace NorthHorizon.Samples.InpcTemplate.Composition
{
    /// <summary>
    /// Defines what components a <see cref="ViewElement"/> should recreate if
    /// its underlying parameters change.
    /// </summary>
    [Flags]
    public enum RecreationOptions
    {
        /// <summary>
        /// If the view or view model changes, its counterpart should not be recreated.
        /// </summary>
        None = 0x0,
        /// <summary>
        /// If the view model changes, the view should also be recreated.
        /// </summary>
        RecreateView = 0x1,
        /// <summary>
        /// If the view changes, the view model should also be recreated.
        /// </summary>
        RecreateViewModel = 0x2
    }
}