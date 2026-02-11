using Hostage.SO;
using System.Collections.Generic;

namespace Hostage.Core
{
    [System.Flags]
    public enum PersonFlag
    {
        None = 0,
        Available = 1 << 0,
        Assistant = 1 << 1,
        Away = 1 << 2,
        Unknown = 1 << 3,
        Occupied = 1 << 4,
        // Add more statuses as needed
    }

    public class Person
    {
        public SOActionPerson SOReference { get; }
        public PersonFlag Flag { get; set; }
        public List<Intel> Intels { get; } = new List<Intel>();

        public Person(SOActionPerson soReference)
        {
            SOReference = soReference;
            Flag = soReference.defaultFlag;
        }

        // helpers
        public bool IsAvailable() => (Flag & PersonFlag.Available) != 0;
        public bool IsAway() => (Flag & PersonFlag.Away) != 0;
        public bool IsUnknown() => (Flag & PersonFlag.Unknown) != 0;
        public bool IsOccupied() => (Flag & PersonFlag.Occupied) != 0;
        public bool IsAssistant() => (Flag & PersonFlag.Assistant) != 0;
        
        public void SetAway() => Flag |= PersonFlag.Away;
        public void SetUnknown() => Flag |= PersonFlag.Unknown;
        public void SetOccupied() => Flag |= PersonFlag.Occupied;
        public void SetAvailable() => Flag |= PersonFlag.Available;
        public void SetAssistant() => Flag |= PersonFlag.Assistant;
        
        public void ClearAway() => Flag &= ~PersonFlag.Away;
        public void ClearUnknown() => Flag &= ~PersonFlag.Unknown;
        public void ClearOccupied() => Flag &= ~PersonFlag.Occupied;
        public void ClearAvailable() => Flag &= ~PersonFlag.Available;
        public void ClearAssistant() => Flag &= ~PersonFlag.Assistant;
        
        public void AddIntel(Intel intel)
        {
            if (!Intels.Contains(intel))
                Intels.Add(intel);
        }

        public bool RemoveIntel(Intel intel) => Intels.Remove(intel);
        public bool HasIntel(Intel intel) => Intels.Contains(intel);
    }
}