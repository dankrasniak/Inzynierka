using System;
using System.Windows.Input;

namespace Inzynierka
{
    public delegate void ButtonAction();
    public delegate bool ExecutionPredicate();

    public class ButtonCommand : ICommand
    {
        private readonly ButtonAction _action;
        private readonly ExecutionPredicate _predicate;

        public ButtonCommand(ButtonAction action, ExecutionPredicate predicate)
        {
            _action = action;
            _predicate = predicate;
        }

        public bool CanExecute(object parameter)
        {
            return _predicate();
        }

        public void Execute(object parameter)
        {
            _action();
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}