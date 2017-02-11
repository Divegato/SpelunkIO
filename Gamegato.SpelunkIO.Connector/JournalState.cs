using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Gamegato.SpelunkIO.Connector
{
    public class JournalState
    {
        public static readonly int NumPlaceEntries = Enum.GetNames(typeof(PlaceEntry)).Length;
        public static readonly int NumMonsterEntries = Enum.GetNames(typeof(MonsterEntry)).Length;
        public static readonly int NumItemEntries = Enum.GetNames(typeof(ItemEntry)).Length;
        public static readonly int NumTrapEntries = Enum.GetNames(typeof(TrapEntry)).Length;
        public static readonly int NumEntries = NumPlaceEntries + NumMonsterEntries + NumItemEntries + NumTrapEntries;

        public bool[] PlaceEntries;
        public bool[] MonsterEntries;
        public bool[] ItemEntries;
        public bool[] TrapEntries;

        public JournalState(bool[] placeEntries, bool[] monsterEntries, bool[] itemEntries, bool[] trapEntries)
        {
            Debug.Assert(placeEntries.Length == NumPlaceEntries);
            Debug.Assert(monsterEntries.Length == NumMonsterEntries);
            Debug.Assert(itemEntries.Length == NumItemEntries);
            Debug.Assert(trapEntries.Length == NumTrapEntries);
            PlaceEntries = placeEntries;
            MonsterEntries = monsterEntries;
            ItemEntries = itemEntries;
            TrapEntries = trapEntries;
        }

        private static int CountUnlockedEntries(bool[] entries)
        {
            return entries.Where(b => b).Count();
        }

        public int NumUnlockedPlaceEntries => CountUnlockedEntries(PlaceEntries);
        public int NumUnlockedMonsterEntries => CountUnlockedEntries(MonsterEntries);
        public int NumUnlockedItemEntries => CountUnlockedEntries(ItemEntries);
        public int NumUnlockedTrapEntries => CountUnlockedEntries(TrapEntries);
        public int NumUnlockedEntries => NumUnlockedPlaceEntries + NumUnlockedMonsterEntries + NumUnlockedItemEntries + NumUnlockedTrapEntries;

        public bool Equals(JournalState o)
        {
            return (new List<Tuple<bool[], bool[]>> {
                new Tuple<bool[], bool[]>(PlaceEntries, o.PlaceEntries),
                new Tuple<bool[], bool[]>(MonsterEntries, o.MonsterEntries),
                new Tuple<bool[], bool[]>(ItemEntries, o.ItemEntries),
                new Tuple<bool[], bool[]>(TrapEntries, o.TrapEntries)
            }).Aggregate(true, (totalResult, entryPair) =>
                totalResult && entryPair.Item1.Length == entryPair.Item2.Length
                    && Enumerable.Range(0, entryPair.Item1.Length).Aggregate(true,
                        (result, currentIndex) => result && entryPair.Item1[currentIndex] == entryPair.Item2[currentIndex]));
        }
    }
}
