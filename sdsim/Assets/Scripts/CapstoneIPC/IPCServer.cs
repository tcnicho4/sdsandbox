using System;
using System.IO.Pipes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Capstone
{
    class IPCServer
    {
        private NamedPipeServerStream mServerPipe = null;
        private IPCActionQueue mActionQueue = null;

        private const string PIPE_NAME = "__CAPSTONE_SIMULATOR_IPC__";

        private class PipeReadOperation
        {
            public byte[] MessageCodeBytes { get; set; }
            public Task<System.Int32> PipeReadTask { get; set; }
        }

        private PipeReadOperation mPipeReadOperation = null;

        private class InvalidMessageException : Exception
        {
            private IPCServerResult mServerResult;
            
            public InvalidMessageException(IPCServerResult serverResult)
            {
                mServerResult = serverResult;
            }

            public IPCServerResult GetServerResult()
            {
                return mServerResult;
            }
        }

        public IPCServer()
        {
            mServerPipe = new NamedPipeServerStream(PIPE_NAME, PipeDirection.InOut, 1, PipeTransmissionMode.Byte);
            mActionQueue = new IPCActionQueue();
        }

        public void RunIPCServerLoop()
        {
            // Wait for the IPCClient to connect.
            IAsyncResult waitResult = mServerPipe.BeginWaitForConnection(null, null);
            mServerPipe.EndWaitForConnection(waitResult);

            while(true)
            {
                try
                {
                    IIPCAction action = CreateActionFromStream();

                    if (action != null)
                        mActionQueue.AddAction(action);

                    ProcessServerResults();
                }
                catch(InvalidMessageException e)
                {
                    try
                    {
                        mServerPipe.Write(BitConverter.GetBytes((System.Int32)e.GetServerResult()), 0, 4);
                    }
                    catch(IOException)
                    {}
                }

                // The server has disconnected from the client.
                catch (ObjectDisposedException)
                {}
                catch (IOException)
                {}
            }
            
        }

        private IIPCAction CreateActionFromStream()
        {
            // Create the asynchronous read task if it does not yet exist.
            if(mPipeReadOperation == null)
            {
                mPipeReadOperation = new PipeReadOperation
                {
                    MessageCodeBytes = new byte[] { 0, 0, 0, 0 },
                    PipeReadTask = null
                };

                mPipeReadOperation.PipeReadTask = mServerPipe.ReadAsync(mPipeReadOperation.MessageCodeBytes, 0, 4);
            }

            if (!mPipeReadOperation.PipeReadTask.IsCompleted)
                return null;

            System.Int32 messageCode = BitConverter.ToInt32(mPipeReadOperation.MessageCodeBytes, 0);
            System.Int32 numBytesRead = mPipeReadOperation.PipeReadTask.Result;

            // Reset the mPipeReadOperation for the next read operation.
            mPipeReadOperation = null;

            if (numBytesRead != 4 || messageCode < 0 || messageCode >= (System.Int32)IPCMessageID.COUNT_OR_ERROR)
               throw new InvalidMessageException(IPCServerResult.ERROR_CORRUPT_STREAM);

            IPCMessageID messageID = (IPCMessageID) messageCode;
            return CreateActionFromMessageID(messageID);
        }

        public void ExecutePendingActions(CapstoneCarController carController)
        {
            mActionQueue.ExecutePendingActions(carController);
        }

        private void ProcessServerResults()
        {
            List<IPCServerResult> serverResults = mActionQueue.ExtractServerResults();

            foreach (IPCServerResult result in serverResults)
                mServerPipe.Write(BitConverter.GetBytes((System.Int32)result), 0, 4);
        }

        private IIPCAction CreateActionFromMessageID(IPCMessageID messageID)
        {
            // Sorry, but I don't know C# well enough to come up with a more elegant solution
            // than this.

            switch(messageID)
            {
                case IPCMessageID.REQUEST_THROTTLE:
                {
                    byte[] messageCodeBytes = { 0, 0, 0, 0 };
                    System.Int32 numBytesRead = mServerPipe.Read(messageCodeBytes, 0, 4);

                    if (numBytesRead != 4)
                        throw new InvalidMessageException(IPCServerResult.ERROR_CORRUPT_STREAM);

                    return new RequestThrottleAction(BitConverter.ToSingle(messageCodeBytes, 0));
                }

                default:
                    return null;
            }
        }
    }
}
