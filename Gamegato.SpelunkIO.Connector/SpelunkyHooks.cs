using System;
using System.IO;
using System.Linq;

namespace Gamegato.SpelunkIO.Connector
{
    public class SpelunkyHooks : IDisposable
    {
        public RawProcess Process { get; private set; }

        public SpelunkyHooks(RawProcess process)
        {
            Process = process;
        }

        public string DeviatedGameSavePath { get; set; }
        public string GameDirectoryPath => Path.GetDirectoryName(Process.FilePath);
        public string GameSavePath => DeviatedGameSavePath ?? GameDirectoryPath + @"\Data\spelunky_save.sav";

        private int Game => Process.ReadInt32(Process.BaseAddress + 0x1384b4);
        private int PlayerBase => Process.ReadInt32(Process.BaseAddress + 0x138558);
        private int Player => Process.ReadInt32(PlayerBase + 0x30);
        private int Timer => Process.ReadInt32(Player + 0x280);
        private int Gfx => Process.ReadInt32(Game + 0x4c);
        public int CharSelectCountdown => Process.ReadInt32(Gfx + 0x122bec);
        public int CharacterHearts => Process.ReadInt32(Player + 0x140);
        public int StageMinutes => Process.ReadInt32(Timer + 0x52BC);
        public SpelunkyState CurrentState => (SpelunkyState)Process.ReadInt32(Game + 0x58);
        public SpelunkyLevel CurrentLevel => (SpelunkyLevel)Process.ReadInt32(Game + 0x4405d4);
        public TunnelManChapter TunnelManChapter => (TunnelManChapter)Process.ReadInt32(Game + 0x445be4);
        public LobbyType CurrentLobbyType => (LobbyType)Process.ReadInt32(Game + 0x445be0);
        public bool Invalidated => Process.HasExited;

        /*
            3 => 1/2/3 Rope(s)
            2 => 1/2/3 Bomb(s)
            1 => $10000/Shotgun/Key
            0 => Nothing
        */
        public int TunnelManRemaining => Process.ReadInt32(Game + 0x445be8);

        private int JournalUnlocksTable => Game + 0x445bec;
        private int JournalPlaceUnlocksTable => JournalUnlocksTable + 0x200;
        private int JournalMonsterUnlocksTable => JournalUnlocksTable + 0x300;
        private int JournalItemUnlocksTable => JournalUnlocksTable + 0x400;
        private int JournalTrapUnlocksTable => JournalUnlocksTable + 0x500;

        private bool[] ReadJournalEntries(int processOffset, int size)
        {
            var entries = new bool[size];
            foreach (var entryIndex in Enumerable.Range(0, size))
            {
                var entryValue = Process.ReadInt32(processOffset + entryIndex * 4);
                if (entryValue == 0)
                    entries[entryIndex] = false;
                else if (entryValue == 1)
                    entries[entryIndex] = true;
                else
                    throw new Exception($"Unexpected journal entry value: {entryValue}");
            }
            return entries;
        }

        public JournalState JournalState =>
            new JournalState(
                ReadJournalEntries(JournalPlaceUnlocksTable, JournalState.NumPlaceEntries),
                ReadJournalEntries(JournalMonsterUnlocksTable, JournalState.NumMonsterEntries),
                ReadJournalEntries(JournalItemUnlocksTable, JournalState.NumItemEntries),
                ReadJournalEntries(JournalTrapUnlocksTable, JournalState.NumTrapEntries));

        public void Dispose()
        {
            Process.Dispose();
        }
    }
}
