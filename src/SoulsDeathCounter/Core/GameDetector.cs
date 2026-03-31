using System;
using System.Diagnostics;
using System.Linq;
using SoulsDeathCounter.Models;

namespace SoulsDeathCounter.Core
{
    public class DetectedGame
    {
        public GameDefinition Definition { get; }
        public Process Process { get; }
        public IntPtr DeathCounterBaseAddress { get; }

        public DetectedGame(GameDefinition definition, Process process, IntPtr baseAddress)
        {
            Definition = definition;
            Process = process;
            DeathCounterBaseAddress = baseAddress;
        }
    }

    public class GameDetector
    {
        private readonly GameDefinition[] _supportedGames;

        public GameDetector()
        {
            _supportedGames = GameDefinition.GetSupportedGames();
        }

        public DetectedGame TryDetectGame()
        {
            foreach (var game in _supportedGames)
            {
                var process = FindGameProcess(game);
                if (process != null)
                {
                    var baseAddress = CalculateBaseAddress(process, game);
                    return new DetectedGame(game, process, baseAddress);
                }
            }
            return null;
        }

        private Process FindGameProcess(GameDefinition game)
        {
            var processes = Process.GetProcessesByName(game.ProcessName);
            var process = processes.FirstOrDefault();

            if (process == null)
                return null;

            if (game.FileDescription != null)
            {
                try
                {
                    var description = process.MainModule?.FileVersionInfo?.FileDescription;
                    if (description != game.FileDescription)
                    {
                        return null;
                    }
                }
                catch
                {
                    return null;
                }
            }

            return process;
        }

        private IntPtr CalculateBaseAddress(Process process, GameDefinition game)
        {
            try
            {
                var moduleBase = process.MainModule?.BaseAddress ?? IntPtr.Zero;
                return IntPtr.Add(moduleBase, game.BaseOffset);
            }
            catch
            {
                return IntPtr.Zero;
            }
        }
    }
}
