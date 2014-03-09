using System;
using System.Linq.Expressions;
using System.Windows.Input;
using NorthHorizon.Samples.InpcTemplate.Core;

namespace NorthHorizon.Samples.InpcTemplate
{
    /// <summary>
    /// Wraps a delegate as an <see cref="ICommand"/> with an expression describing when the command can be executed.
    /// </summary>
    public sealed class ExpressionCommand : ExpressionCommandBase
    {
        private readonly Action _onExecute;
        private readonly Func<bool> _onCanExecute;
        private bool? _canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionCommand"/> class.
        /// </summary>
        /// <param name="onExecute">The action to call when the command is executed.</param>
        /// <param name="onCanExecute">An expression describing when the command can be executed.</param>
        public ExpressionCommand(Action onExecute, Expression<Func<bool>> onCanExecute)
        {
            if (onExecute == null) throw new ArgumentNullException("onExecute");
            if (onCanExecute == null) throw new ArgumentNullException("onCanExecute");

            _onExecute = onExecute;
            _onCanExecute = onCanExecute.Compile();

            SubscribeToChanges(onCanExecute);
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
        /// Called when <see cref="CanExecute" /> should be queried again.
        /// </summary>
        protected override void OnCanExecuteChanged()
        {
            _canExecute = null;

            base.OnCanExecuteChanged();
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
            if (_canExecute == null)
                _canExecute = _onCanExecute();

            return _canExecute.Value;
        }
    }

    /// <summary>
    /// Wraps a delegate with a parameter as an <see cref="ICommand"/> with an expression describing when the command can be executed.
    /// </summary>
    public sealed class ExpressionCommand<T> : ExpressionCommandBase
    {
        private readonly Action<T> _onExecute;
        private readonly Func<T, bool> _onCanExecute;

        private bool? _canExecute;
        private WeakReference _lastParameter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionCommand&lt;T&gt;"/> class.
        /// </summary>
        /// <param name="onExecute">The action to call when the command is executed.</param>
        /// <param name="onCanExecute">An expression describing when the command can be executed.</param>
        public ExpressionCommand(Action<T> onExecute, Expression<Func<T, bool>> onCanExecute)
        {
            if (onExecute == null) throw new ArgumentNullException("onExecute");
            if (onCanExecute == null) throw new ArgumentNullException("onCanExecute");

            _onExecute = onExecute;
            _onCanExecute = onCanExecute.Compile();
            SubscribeToChanges(onCanExecute);
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
        /// Called when <see cref="CanExecute" /> should be queried again.
        /// </summary>
        protected override void OnCanExecuteChanged()
        {
            _canExecute = null;

            base.OnCanExecuteChanged();
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
            if (_canExecute != null &&
                (_lastParameter == null && parameter == null ||
                 _lastParameter != null && _lastParameter.IsAlive && parameter == _lastParameter.Target))
                return _canExecute.Value;

            _lastParameter = new WeakReference(parameter);
            T typedParameter;
            _canExecute = TryConvert(parameter, out typedParameter) && _onCanExecute(typedParameter);

            return _canExecute.Value;
        }
    }
}