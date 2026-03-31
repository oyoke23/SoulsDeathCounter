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
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool ReadProcessMemory(
            IntPtr hProcess,
            IntPtr lpBaseAddress,
            byte[] lpBuffer,
            int dwSize,
            out int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);

        public bool IsAttached => _processHandle != IntPtr.Zero;

        public bool Is64BitProcess { get; private set; }

        public bool Attach(Process process)
        {
            if (process == null || process.HasExited)
                return false;

            Detach();

            _processHandle = OpenProcess(PROCESS_VM_READ | PROCESS_QUERY_INFORMATION, false, process.Id);

            if (_processHandle == IntPtr.Zero)
                return false;

            Is64BitProcess = !IsRunningUnderWow64(process);
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

        public int ReadInt32(IntPtr address)
        {
            byte[] buffer = new byte[4];
            if (ReadProcessMemory(_processHandle, address, buffer, buffer.Length, out _))
                return BitConverter.ToInt32(buffer, 0);
            return 0;
        }

        public long ReadInt64(IntPtr address)
        {
            byte[] buffer = new byte[8];
            if (ReadProcessMemory(_processHandle, address, buffer, buffer.Length, out _))
                return BitConverter.ToInt64(buffer, 0);
            return 0;
        }

        public IntPtr ReadPointer(IntPtr address)
        {
            return Is64BitProcess
                ? (IntPtr)ReadInt64(address)
                : (IntPtr)ReadInt32(address);
        }

        public IntPtr FollowPointerChain(IntPtr baseAddress, int[] offsets)
        {
            IntPtr current = baseAddress;

            for (int i = 0; i < offsets.Length; i++)
            {
                if (offsets[i] == 0 && i > 0)
                    break;

                if (i < offsets.Length - 1 || offsets.Length == 1)
                {
                    current = ReadPointer(current);
                    if (current == IntPtr.Zero)
                        return IntPtr.Zero;
                }

                if (i < offsets.Length - 1)
                    current = IntPtr.Add(current, offsets[i]);
            }

            if (offsets.Length > 1)
                current = IntPtr.Add(current, offsets[offsets.Length - 1]);

            return current;
        }

        private bool IsRunningUnderWow64(Process process)
        {
            if (!Environment.Is64BitOperatingSystem)
                return false;

            if (IsWow64Process(process.Handle, out bool isWow64))
                return isWow64;

            return false;
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
