using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using System.Security;

namespace BclExtension
{
    // .NET 4.0 이하에서 Console.IsInputRedirected 구현
    // ; https://www.sysnet.pe.kr/2/0/12590

    class ConsoleHelper
    {
        [DllImport("kernel32.dll")]
        internal static extern int GetFileType(SafeFileHandle handle);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetStdHandle(StdHandle std);

        [DllImport("kernel32.dll", SetLastError = true)]
        internal static extern bool GetConsoleMode(IntPtr hConsoleHandle, out int mode);

        private enum StdHandle { Stdin = -10, Stdout = -11, Stderr = -12 };

        [SecuritySafeCritical]
        public static bool IsInputHandleRedirected()
        {
            IntPtr ioHandle = GetStdHandle(StdHandle.Stdin);

            SafeFileHandle handle = new SafeFileHandle(ioHandle, ownsHandle: false);
            int fileType = GetFileType(handle);
            if ((fileType & 2) != 2)
            {
                return true;
            }

            bool consoleMode = GetConsoleMode(ioHandle, out _);
            return !consoleMode;
        }
    }
}
