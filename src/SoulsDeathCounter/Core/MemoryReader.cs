using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SoulsDeathCounter.Core
{
    public sealed class MemoryReader : IDisposable
    {
        private const int PROCESS_VM_READ = 0x0010;
        private const int PROCESS_QUERY_INFORMATION = 0x0400;

        private IntPtr _processHandle;
        private bool _disposed;

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            int dwSize,
            ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool IsWow64Process(IntPtr hProcess, ref bool wow64Process);

        public bool IsAttached => _processHandle != IntPtr.Zero;

        public bool Is64BitProcess { get; private set; }

        public bool Attach(Process process)
        {
            if (process == null || process.HasExited)
                return false;

            Detach();

            _processHandle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, false, process.Id);

            if (_processHandle == IntPtr.Zero)
                return false;

            bool isWow64 = false;
            IsWow64Process(_processHandle, ref isWow64);
            Is64BitProcess = !isWow64;

            return true;
        }

        public void Detach()
        {
            if (_processHandle != IntPtr.Zero)
            {
                CloseHandle(_processHandle);
                _processHandle = IntPtr.Zero;
            }
        }

        public int ReadDeathCount(IntPtr baseAddress, int[] offsets)
        {
            long address = baseAddress.ToInt64();
            byte[] buffer = new byte[8];
            int discard = 0;

            foreach (int offset in offsets)
            {
                if (address == 0)
                    return -1;

                address += offset;

                if (!ReadProcessMemory(_processHandle, (IntPtr)address, buffer, 8, ref discard))
                    return -1;

                address = Is64BitProcess
                    ? BitConverter.ToInt64(buffer, 0)
                    : BitConverter.ToInt32(buffer, 0);
            }

            return (int)address;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Detach();
                _disposed = true;
            }
        }
    }
}
