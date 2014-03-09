using System;
using System.Windows.Input;
using Igniter.Core;

namespace Igniter
{
    /// <summary>
    /// Wraps a delegate as an <see cref="ICommand"/>.
    /// </summary>
    public sealed class DelegateCommand : DelegateCommandBase
    {
        private readonly Action _onExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand"/> class.
        /// </summary>
        /// <param name="onExecute">The action to call when the command is executed.</param>
        /// <param name="canExecute">The initial state of executability for this command.</param>
        public DelegateCommand(Action onExecute, bool canExecute = true) : base(canExecute)
        {
            if (onExecute == null) throw new ArgumentNullException("onExecute");

            _onExecute = onExecute;
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public override void Execute(object parameter)
        {
            _onExecute();
        }
    }

    /// <summary>
    /// Wraps a delegate with a parameter as an <see cref="ICommand"/>.
    /// </summary>
    /// <typeparam name="T">The type of the command parameter.</typeparam>
    public sealed class DelegateCommand<T> : DelegateCommandBase
    {
        private readonly Action<T> _onExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommand&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="onExecute">The action to call when the command is executed.</param>
        /// <param name="canExecute">The initial state of executability for this command.</param>
        public DelegateCommand(Action<T> onExecute, bool canExecute = true)
            : base(canExecute)
        {
            if (onExecute == null) throw new ArgumentNullException("onExecute");

            _onExecute = onExecute;
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public override void Execute(object parameter)
        {
            T typedParameter;
            if (TryConvert(parameter, out typedParameter))
                _onExecute(typedParameter);
        }

        /// <summary>
        /// Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        /// <returns>
        /// true if this command can be executed; otherwise, false.
        /// </returns>
        public override bool CanExecute(object parameter)
        {
            T typedParameter;
            return base.CanExecute(parameter) && TryConvert(parameter, out typedParameter);
        }
    }
}