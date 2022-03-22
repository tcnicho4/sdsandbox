using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone
{
    class RequestThrottleAction : IIPCAction
    {
        private float mThrottleAmount;

        public RequestThrottleAction(float throttleAmount)
        {
            mThrottleAmount = throttleAmount;
        }

        public IPCServerResult ExecuteAction(CapstoneCarController carController)
        {
            // The throttle amount must be in the range [0.0f, 1.0f].
            if (mThrottleAmount < 0.0f)
                mThrottleAmount = 0.0f;
            else if (mThrottleAmount > 1.0f)
                mThrottleAmount = 1.0f;
            
            carController.GetCar().RequestThrottle(mThrottleAmount);

            return IPCServerResult.OK;
        }
    }
}
