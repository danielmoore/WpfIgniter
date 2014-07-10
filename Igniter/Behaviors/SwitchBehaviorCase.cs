using System.Windows;
using Igniter.Core;

namespace Igniter.Behaviors
{
    public abstract class SwitchBehaviorCase : DependencyObject
    {
        #region object When { get; set; }

        /// <summary>
        /// Identifies the <see cref="When"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WhenProperty =
            SwitchCaseEvaluator.WhenProperty.AddOwner(typeof(SwitchBehaviorCase));

        /// <summary>
        /// Gets or sets the When.
        /// </summary>
        public object When
        {
            get { return (object)GetValue(WhenProperty); }
            set { SetValue(WhenProperty, value); }
        }

        #endregion

        protected internal SwitchBehavior OwningSwitchBehavior { get; internal set; }

        public abstract void ApplyState(DependencyObject obj);
    }
}