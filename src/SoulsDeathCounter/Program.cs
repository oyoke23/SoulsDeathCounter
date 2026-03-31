using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace SoulsDeathCounter
{
    class Game
    {
        public readonly string name;
        public readonly string displayName;
        public readonly int[] offsets;

        public Game(string name, string displayName, int[] offsets)
        {
            this.name = name;
            this.displayName = displayName;
            this.offsets = offsets;
        }
    }

    class Program
    {
        const int PROCESS_WM_READ = 0x0010;
        const int PROCESS_QUERY_INFORMATION = 0x0400;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool IsWow64Process(IntPtr hProcess, ref bool Wow64Process);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ReadProcessMemory(
            IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr hObject);

        static readonly Game[] games =
        {
            new Game("DarkSoulsRemastered", "Dark Souls Remastered", new int[] {0x1C8A530, 0x98}),
            new Game("DARKSOULS", "Dark Souls: Prepare To Die", new int[] {0xF78700, 0x5C}),
            new Game("DarkSoulsIII", "Dark Souls III", new int[] {0x47572B8, 0x98}),
            new Game("DarkSoulsII", "Dark Souls II", new int[] {0x16148F0, 0xD0, 0x490, 0x104}),
            new Game("Sekiro", "Sekiro: Shadows Die Twice", new int[] {0x3D5AAC0, 0x90}),
            new Game("eldenring", "Elden Ring", new int[] {0x3D5DF38, 0x94})
        };

        static readonly string DeathsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "deaths.txt");

        static void WriteDeaths(int value)
        {
            try
            {
                File.WriteAllText(DeathsFile, value.ToString());
            }
            catch { }
        }

        static bool ReadMemory(IntPtr handle, IntPtr baseAddress, bool isX64, int[] offsets, ref int value)
        {
            long address = baseAddress.ToInt64();
            byte[] buffer = new byte[8];
            int discard = 0;

            foreach (int offset in offsets)
            {
                if (address == 0)
                    return false;

                address += offset;

                if (!ReadProcessMemory(handle, (IntPtr)address, buffer, 8, ref discard))
                    return false;

                address = isX64 ? BitConverter.ToInt64(buffer, 0) : BitConverter.ToInt32(buffer, 0);
            }

            value = (int)address;
            return true;
        }

        static bool FindGame(ref Process proc, ref Game game)
        {
            foreach (Game g in games)
            {
                Process[] processes = Process.GetProcessesByName(g.name);
                if (processes.Length > 0)
                {
                    proc = processes[0];
                    game = g;
                    return true;
                }
            }
            return false;
        }

        [STAThread]
        static void Main()
        {
            Console.Title = "Souls Death Counter";
            Console.WriteLine("╔════════════════════════════════════════╗");
            Console.WriteLine("║        SOULS DEATH COUNTER v2.1        ║");
            Console.WriteLine("╠════════════════════════════════════════╣");
            Console.WriteLine("║  El contador se guarda en deaths.txt   ║");
            Console.WriteLine("║  Usa ese archivo en OBS como texto     ║");
            Console.WriteLine("╚════════════════════════════════════════╝");
            Console.WriteLine();

            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            while (true)
            {
                WriteDeaths(0);
                Console.WriteLine("Buscando juego...");

                Process proc = null;
                Game game = null;

                while (!FindGame(ref proc, ref game))
                {
                    Thread.Sleep(500);
                }

                Console.WriteLine($"Encontrado: {game.displayName}");

                IntPtr handle = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_WM_READ, false, proc.Id);
                IntPtr baseAddress = proc.MainModule.BaseAddress;
                int oldValue = 0, value = 0;

                bool isWow64 = false;
                IsWow64Process(handle, ref isWow64);
                bool isX64 = !isWow64;

                Console.WriteLine($"Proceso {(isX64 ? "64" : "32")} bits");
                Console.WriteLine();

                while (!proc.HasExited)
                {
                    if (ReadMemory(handle, baseAddress, isX64, game.offsets, ref value))
                    {
                        if (value != oldValue)
                        {
                            oldValue = value;
                            WriteDeaths(value);
                            Console.WriteLine($"Muertes: {value}");
                        }
                    }
                    Thread.Sleep(500);
                }

                CloseHandle(handle);
                Console.WriteLine("El juego se ha cerrado.");
                Console.WriteLine();
                Thread.Sleep(2000);
            }
        }
    }
}
