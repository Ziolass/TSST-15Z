using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NetworkClientNode.ViewModelUtils
{
    public class ExternalCommand : ICommand
    {
        private Action What;
        private Func<bool> When;
        private bool WhenBool;
        public ExternalCommand(Action what, Func<bool> when)
        {
            What = what;
            When = when;
        }
        public ExternalCommand(Action what, bool when)
        {
            What = what;
            WhenBool = when;
            When = StaticWhen;
        }
        public bool CanExecute(object parameter)
        {
            return When();
        }
        public void Execute(object parameter)
        {
            What();
        }
        public bool StaticWhen()
        {
            return WhenBool;
        }


        public event EventHandler CanExecuteChanged;
    }
}
