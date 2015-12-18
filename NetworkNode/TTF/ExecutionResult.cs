using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.TTF
{
        public class ExecutionResult
        {
            public string Msg { get; private set; }
            public bool Result { get; private set; }
            public ExecutionResult(bool result, string msg)
            {
                Result = result;
                Msg = msg;
            }
        }
}
