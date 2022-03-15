using System;
using System.IO.Pipes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capstone
{
    class IPCServer
    {
        private NamedPipeServerStream mServerPipe = null;
        private IPCMessageQueue mMessageQueue = null;

        private const string PIPE_NAME = "__CAPSTONE_SIMULATOR_IPC__";

        public IPCServer()
        {
            mServerPipe = new NamedPipeServerStream(PIPE_NAME, PipeDirection.InOut, 1, PipeTransmissionMode.Message);
            mMessageQueue = new IPCMessageQueue();
        }

        public void RunIPCServerLoop()
        {
            // Wait for the IPCClient to connect.
            mServerPipe.WaitForConnection();

            while(true)
            {
                try
                {

                }
            }
            
        }

        private IIPCAction CreateActionFromStream()
        {
            byte[] messageCodeBytes = { 0, 0, 0, 0 };
            System.Int32 numBytesRead = mServerPipe.Read(messageCodeBytes, 0, 4);

            if (numBytesRead == 0)
                return null;

            System.Int32 messageCode = BitConverter.ToInt32(messageCodeBytes, 0);
        }

        public void ExecutePendingActions(CapstoneCarController carController)
        {
            mMessageQueue.ExecutePendingActions(carController);
        }
    }
}
