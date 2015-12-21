using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace WireCloud.ViewModelUtils
{
    public class ExternalCommand : ICommand
    {
        private Action What;
        private Func<bool> When;
        public ExternalCommand(Action what, Func<bool> when)
        {
            What = what;
            When = when;
        }
        public bool CanExecute(object parameter)
        {
            return When();
        }
        public void Execute(object parameter)
        {
            What();
        }


        public event EventHandler CanExecuteChanged;
    }
}
