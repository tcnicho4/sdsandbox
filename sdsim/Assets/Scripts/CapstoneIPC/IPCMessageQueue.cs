using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone
{
    class IPCMessageQueue
    {
        private Queue<IIPCAction> mActionQueue = new Queue<IIPCAction>();
        private object mCritSection = new object();

        public IPCMessageQueue()
        {}

        public void AddAction(IIPCAction action)
        {
            lock(mCritSection)
            {
                mActionQueue.Enqueue(action);
            }    
        }

        public void ExecutePendingActions(CapstoneCarController carController)
        {
            // Rather than taking objects from the queue in between locks, we will lock
            // the critical section, execute all of the actions in the queue, and then
            // unlock the critical section.
            //
            // This will prevent the calling thread from potentially never being able
            // to leave this function.
            lock(mCritSection)
            {
                while(mActionQueue.Count > 0)
                {
                    IIPCAction currAction = mActionQueue.Dequeue();
                    currAction.ExecuteAction(carController);
                }
            }
        }
    }
}
