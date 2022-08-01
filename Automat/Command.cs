using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Automat
{
    internal class Command : ICommand
    {
        public event EventHandler? CanExecuteChanged;

        Action _action;
        Action<object> _argAction;

        public Command(Action action)
        {
            _action = action;
        }

        public Command(Action<object> action)
        {
            _argAction = action;
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            if (_action == null)
                _argAction.Invoke(parameter);

            else 
                _action();
        }
    }
}
