using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Capstone
{
    class Win32Interop
    {
        // Traditionally, the BOOL data type was implemented as a std::int32_t in
        // the Win32 API. This is because C (at the time) did not have a boolean
        // primitive. (Fun Fact: After God-knows-how-many years, C will finally
        // have a boolean primitive in C23!)
        public enum BOOL : System.Int32
        {
            FALSE = 0,
            TRUE = 1
        }

        public enum EventAccessRights : System.UInt32
        {
            DELETE              = 0x00010000,
            READ_CONTROL        = 0x00020000,
            SYNCHRONIZE         = 0x00100000,
            WRITE_DAC           = 0x00040000,
            WRITE_OWNER         = 0x00080000,

            EVENT_ALL_ACCESS    = 0x1F0003,
            EVENT_MODIFY_STATE  = 0x0002
        }

        public static readonly IntPtr INVALID_HANDLE_VALUE = (IntPtr) (-1);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern IntPtr OpenEventW(EventAccessRights dwDesiredAccess, BOOL bInheritHandle, string lpName);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern BOOL CloseHandle(IntPtr hObject);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern BOOL SetEvent(IntPtr hEvent);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern BOOL ConnectNamedPipe(SafeHandle hNamedPipe, IntPtr lpOverlapped);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern System.UInt32 GetLastError();

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern BOOL ReadFile(SafeHandle hFile, byte[] lpBuffer, System.UInt32 nNumberOfBytesToRead, ref System.UInt32 lpNumberOfBytesRead, IntPtr lpOverlapped);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern BOOL WriteFile(SafeHandle hFile, byte[] lpBuffer, System.UInt32 nNumberOfBytesToWrite, ref System.UInt32 lpNumberOfBytesWritten, IntPtr lpOverlapped);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern BOOL PeekNamedPipe(SafeHandle hNamedPipe, byte[] lpBuffer, System.UInt32 mBufferSize, ref System.UInt32 lpBytesRead, ref System.UInt32 lpTotalBytesAvail, ref System.UInt32 lpBytesLeftThisMessage);
    }
}
