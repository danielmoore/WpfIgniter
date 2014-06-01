using System;
using System.Linq.Expressions;
using System.Windows.Input;
using Igniter.Core;

namespace Igniter
{
    /// <summary>
    /// Wraps a delegate as an <see cref="ICommand"/> with an expression describing when the command can be executed.
    /// </summary>
    public sealed class ExpressionCommand : ExpressionCommandBase
    {
        private readonly Action _onExecute;
        private readonly Func<bool> _onCanExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionCommand"/> class.
        /// </summary>
        /// <param name="onExecute">The action to call when the command is executed.</param>
        /// <param name="onCanExecute">An expression describing when the command can be executed.</param>
        public ExpressionCommand(Action onExecute, Expression<Func<bool>> onCanExecute) : base(onCanExecute)
        {
            if (onExecute == null) throw new ArgumentNullException("onExecute");
            if (onCanExecute == null) throw new ArgumentNullException("onCanExecute");

            _onExecute = onExecute;
            _onCanExecute = onCanExecute.Compile();
        }

        /// <summary>
        /// Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">Data used by the command.  If the command does not require data to be passed, this object can be set to null.</param>
        public override void Execute(object parameter)
        {
            _onExecute();
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
            // NOTE: not caching for consistency with ExpressionCommand<T>.CanExecute.

            return _onCanExecute();
        }
    }

    /// <summary>
    /// Wraps a delegate with a parameter as an <see cref="ICommand"/> with an expression describing when the command can be executed.
    /// </summary>
    public sealed class ExpressionCommand<T> : ExpressionCommandBase
    {
        private readonly Action<T> _onExecute;
        private readonly Func<T, bool> _onCanExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionCommand&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="onExecute">The action to call when the command is executed.</param>
        /// <param name="onCanExecute">An expression describing when the command can be executed.</param>
        public ExpressionCommand(Action<T> onExecute, Expression<Func<T, bool>> onCanExecute) : base(onCanExecute)
        {
            if (onExecute == null) throw new ArgumentNullException("onExecute");
            if (onCanExecute == null) throw new ArgumentNullException("onCanExecute");

            _onExecute = onExecute;
            _onCanExecute = onCanExecute.Compile();
        }

        /// <summary>
        /// Called when the command parameter has changed.
        /// </summary>
        protected override void OnParameterChanged()
        {
            OnCanExecuteChanged();
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
            // Sadly, we can't cache the result of CanExecute because we don't know how many
            // elements are referencing this command and each can have a different parameter.

            T typedParameter;
            return TryConvert(parameter, out typedParameter) && _onCanExecute(typedParameter);
        }
    }
}