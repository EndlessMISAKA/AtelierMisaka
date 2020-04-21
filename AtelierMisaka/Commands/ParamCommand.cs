using System;

namespace AtelierMisaka.Commands
{
    public class ParamCommand<T> : BaseCommand
    {
        private Action<T> _execute;

        public ParamCommand(Action<T> execute) : this(execute, null) { }

        public ParamCommand(Action<T> execute, Func<bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public override void Execute(object parameter)
        {
            _execute((T)parameter);
        }
    }
}
