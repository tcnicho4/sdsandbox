using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone
{
    // We specify the underlying type of System.Int32 here because this is the default
    // underlying type of enums in C++.
    enum IPCMessageID : System.Int32
    {
        REQUEST_THROTTLE,
        
        COUNT_OR_ERROR
    }
}
