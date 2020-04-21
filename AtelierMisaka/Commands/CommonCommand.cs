using System;

namespace AtelierMisaka.Commands
{
    public class CommonCommand : BaseCommand
    {
        private Action _execute;

        public CommonCommand(Action execute) : this(execute, null) { }

        public CommonCommand(Action execute, Func<bool> canExecute)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public override void Execute(object parameter)
        {
            _execute();
        }
    }
}
