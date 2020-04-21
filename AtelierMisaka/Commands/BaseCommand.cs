using System;
using System.Windows.Input;

namespace AtelierMisaka.Commands
{
    public abstract class BaseCommand : ICommand
    {
        protected Func<bool> _canExecute;

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return (_canExecute == null) ? true : _canExecute();
        }

        public abstract void Execute(object parameter);
    }
}
