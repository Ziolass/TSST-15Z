﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetworkNode.Frame
{
    interface IContent
    {
        ContentType Type { get; private set; }
    }
}
