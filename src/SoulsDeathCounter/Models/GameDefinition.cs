using System;

namespace SoulsDeathCounter.Models
{
    public class GameDefinition
    {
        public string Name { get; }
        public string ProcessName { get; }
        public int BaseOffset { get; }
        public int[] PointerOffsets { get; }
        public string FileDescription { get; }

        public GameDefinition(string name, string processName, int baseOffset, int[] pointerOffsets, string fileDescription = null)
        {
            Name = name;
            ProcessName = processName;
            BaseOffset = baseOffset;
            PointerOffsets = pointerOffsets;
            FileDescription = fileDescription;
        }

        public static GameDefinition[] GetSupportedGames()
        {
            return new[]
            {
                new GameDefinition(
                    name: "Dark Souls Remastered",
                    processName: "DarkSoulsRemastered",
                    baseOffset: 0x1D278F0,
                    pointerOffsets: new[] { 0x98 }
                ),
                new GameDefinition(
                    name: "Dark Souls: Prepare To Die",
                    processName: "DarkSouls",
                    baseOffset: 0xF78700,
                    pointerOffsets: new[] { 0x5C }
                ),
                new GameDefinition(
                    name: "Dark Souls III",
                    processName: "DarkSoulsIII",
                    baseOffset: 0x4740178,
                    pointerOffsets: new[] { 0x98 }
                ),
                new GameDefinition(
                    name: "Dark Souls II",
                    processName: "DarkSoulsII",
                    baseOffset: 0x11493F4,
                    pointerOffsets: new[] { 0x74, 0x378, 0x1A0 },
                    fileDescription: "DARK SOULS Ⅱ"
                ),
                new GameDefinition(
                    name: "Dark Souls II: Scholar of the First Sin",
                    processName: "DarkSoulsII",
                    baseOffset: 0x160B8D0,
                    pointerOffsets: new[] { 0xD0, 0x490, 0x1A4 }
                ),
                new GameDefinition(
                    name: "Sekiro: Shadows Die Twice",
                    processName: "Sekiro",
                    baseOffset: 0x3D5AAC0,
                    pointerOffsets: new[] { 0x90 }
                ),
                new GameDefinition(
                    name: "Elden Ring",
                    processName: "eldenring",
                    baseOffset: 0x3CD5DF8,
                    pointerOffsets: new[] { 0x94 }
                )
            };
        }
    }
}
