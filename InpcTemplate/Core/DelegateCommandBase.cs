using System.Windows.Input;

namespace NorthHorizon.Samples.InpcTemplate.Core
{
    /// <summary>
    /// A base implementation of <see cref="ICommand"/> that allows user code to set the executability of a command.
    /// </summary>
    public abstract class DelegateCommandBase : CommandBase
    {
        private bool _canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateCommandBase"/> class.
        /// </summary>
        /// <param name="canExecute">The initial state of executability for this command.</param>
        protected DelegateCommandBase(bool canExecute)
        {
            _canExecute = canExecute;
        }

        /// <summary>
        /// Sets a flag enabling or disabling the ability for this command to be executed.
        /// </summary>
        /// <param name="value">if set to <c>true</c> allows execution. <c>false</c> otherwise.</param>
        public void SetCanExecute(bool value)
        {
            if (_canExecute == value) return;

            _canExecute = value;
            OnCanExecuteChanged();
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
            return _canExecute;
        }
    }
}