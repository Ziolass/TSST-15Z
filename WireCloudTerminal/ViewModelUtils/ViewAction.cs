using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WireCloud.ViewModelUtils
{
    public class ViewAction
    {
        public Action LinkAction { get; private set; }
        public String Label { get; private set; }

        public ViewAction(String label, Action action)
        {
            Label = label;
            LinkAction = action;
        }
    }
}
