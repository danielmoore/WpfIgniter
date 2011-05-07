using System;
using System.Windows.Input;

namespace InpcTemplate.Tests.Live
{
    public class DelegateCommand : ICommand
    {
        private readonly Action _execute;

        public DelegateCommand(Action execute)
        {
            _execute = execute;
        }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            _execute();
        }

        #endregion
    }

    public class DelegateCommand<T> : ICommand
    {
        private readonly Action<T> _execute;

        public DelegateCommand(Action<T> execute)
        {
            _execute = execute;
        }

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (parameter == null || parameter is T)
                _execute((T)parameter);
        }

        #endregion
    }
}
