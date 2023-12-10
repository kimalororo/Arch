using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ClientWPF.Command
{
    internal class Command:ICommand
    {
        public event EventHandler CanExecuteChanged;
        Action<object> action;
        public Command(Action<object> action)
        {
            this.action = action;
        }
        public bool CanExecute(object parameter) => true;

        public void Execute(object parameter) => action?.Invoke(parameter);
    }
}
