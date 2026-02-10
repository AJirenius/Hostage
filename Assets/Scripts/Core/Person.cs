using Hostage.SO;
using System.Collections.Generic;

namespace Hostage.Core
{
    [System.Flags]
    public enum PersonStatus
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
        public PersonStatus Status { get; set; }
        public List<Intel> Intels { get; } = new List<Intel>();

        public Person(SOActionPerson soReference)
        {
            SOReference = soReference;
            Status = soReference.defaultStatus;
        }

        // helpers
        public bool IsAvailable() => (Status & PersonStatus.Available) != 0;
        public bool IsAway() => (Status & PersonStatus.Away) != 0;
        public bool IsUnknown() => (Status & PersonStatus.Unknown) != 0;
        public bool IsOccupied() => (Status & PersonStatus.Occupied) != 0;
        public bool IsAssistant() => (Status & PersonStatus.Assistant) != 0;
        
        public void SetAway() => Status |= PersonStatus.Away;
        public void SetUnknown() => Status |= PersonStatus.Unknown;
        public void SetOccupied() => Status |= PersonStatus.Occupied;
        public void SetAvailable() => Status |= PersonStatus.Available;
        public void SetAssistant() => Status |= PersonStatus.Assistant;
        
        public void ClearAway() => Status &= ~PersonStatus.Away;
        public void ClearUnknown() => Status &= ~PersonStatus.Unknown;
        public void ClearOccupied() => Status &= ~PersonStatus.Occupied;
        public void ClearAvailable() => Status &= ~PersonStatus.Available;
        public void ClearAssistant() => Status &= ~PersonStatus.Assistant;
        
        public void AddIntel(Intel intel)
        {
            if (!Intels.Contains(intel))
                Intels.Add(intel);
        }

        public bool RemoveIntel(Intel intel) => Intels.Remove(intel);
        public bool HasIntel(Intel intel) => Intels.Contains(intel);
    }
}