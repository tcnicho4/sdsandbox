using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone
{
    interface IIPCAction
    {
        public void ExecuteAction(CapstoneCarController carController);
    }
}
