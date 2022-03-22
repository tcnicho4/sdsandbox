using System;
using System.IO.Pipes;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.ComponentModel;

namespace Capstone
{
    class IPCServer
    {
        private NamedPipeServerStream mServerPipe = null;
        private IPCActionQueue mActionQueue = null;

        private const string PIPE_NAME = "__CAPSTONE_SIMULATOR_IPC__";

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
            // Wait for the client to connect.
            mServerPipe.WaitForConnection();

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
                    {
                        break;
                    }
                }

                // The server has disconnected from the client.
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (IOException)
                {
                    break;
                }
            }
        }

        private IIPCAction CreateActionFromStream()
        {
            // I don't know why, but unless we call PeekNamedPipe() to make sure that there are
            // bytes present in the pipe before calling ReadFile() (which is internally called
            // by NamedPipeServerStream::Read()), the thread running this function will hang
            // on the second call to ReadFile().
            //
            // This doesn't really seem to be well-documented anywhere, unless I am just missing
            // something. The NamedPipeServerStream class does not appear to offer what we need,
            // so we need to make a raw Win32 call using Win32Interop.PeekNamedPipe().
            if (!DoesPipeHaveBytesToRead())
                return null;

            byte[] messageCodeBytes = { 0, 0, 0, 0 };
            System.Int32 numBytesRead = mServerPipe.Read(messageCodeBytes, 0, 4);

            System.Int32 messageCode = BitConverter.ToInt32(messageCodeBytes, 0);

            if (numBytesRead != 4 || messageCode < 0 || messageCode >= (System.UInt32)IPCMessageID.COUNT_OR_ERROR)
               throw new InvalidMessageException(IPCServerResult.ERROR_CORRUPT_STREAM);

            IPCMessageID messageID = (IPCMessageID) messageCode;
            return CreateActionFromMessageID(messageID);
        }

        public void ExecutePendingActions(CapstoneCarController carController)
        {
            mActionQueue.ExecutePendingActions(carController);
        }

        private bool DoesPipeHaveBytesToRead()
        {
            System.UInt32 numBytesAvailable = 0;
            byte[] lpBuffer = null;
            System.UInt32 dummyInteger = 0;

            Win32Interop.BOOL peekResult = Win32Interop.PeekNamedPipe(
                mServerPipe.SafePipeHandle,
                lpBuffer,
                0,
                ref dummyInteger,
                ref numBytesAvailable,
                ref dummyInteger
            );

            if (peekResult == Win32Interop.BOOL.FALSE)
                throw new IOException();

            return (numBytesAvailable != 0);
        }

        private void ProcessServerResults()
        {
            List<IPCServerResult> serverResults = mActionQueue.ExtractServerResults();

            foreach (IPCServerResult result in serverResults)
                mServerPipe.Write(BitConverter.GetBytes((System.Int32)result), 0, 4);

            if(serverResults.Count > 0)
            {
                // If we wrote any results to the client, then let it know by signalling
                // the appropriate event.
                const string SERVER_RESPONSE_EVENT_NAME = "__CAPSTONE_IPC_SERVER_RESPONSE_EVENT__";

                // Call the native Win32 function to get access to the event handle.
                IntPtr hServerResponseEvent = Win32Interop.OpenEventW(Win32Interop.EventAccessRights.DELETE | Win32Interop.EventAccessRights.EVENT_MODIFY_STATE, Win32Interop.BOOL.FALSE, SERVER_RESPONSE_EVENT_NAME);

                if (hServerResponseEvent == IntPtr.Zero)
                    throw new IOException();

                // Signal the event, since we have written to the pipe. (Events can be
                // used across processes in the Win32 API.)
                Win32Interop.BOOL result = Win32Interop.SetEvent(hServerResponseEvent);

                if (result == Win32Interop.BOOL.FALSE)
                    throw new IOException();

                // There's no RAII in C#, so we need to manually close the handle.
                result = Win32Interop.CloseHandle(hServerResponseEvent);

                if (result == Win32Interop.BOOL.FALSE)
                    throw new IOException();
            }
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
