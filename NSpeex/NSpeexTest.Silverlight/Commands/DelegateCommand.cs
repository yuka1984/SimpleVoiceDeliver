using System;
using System.Windows.Input;

namespace NSpeexTest.Silverlight.Commands
{
    public class DelegateCommand<T> : ICommand
    {
        private readonly Func<T, bool> canExecute;
        private readonly Action<T> execute;

        public DelegateCommand(Action<T> execute)
        {
            canExecute = T => true;
            this.execute = execute;
        }

        public DelegateCommand(Func<T, bool> canExecute, Action<T> execute)
        {
            this.canExecute = canExecute;
            this.execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute.Invoke((T) parameter);
        }

        public void Execute(object parameter)
        {
            execute.Invoke((T) parameter);
        }

        public event EventHandler CanExecuteChanged;
    }
}
