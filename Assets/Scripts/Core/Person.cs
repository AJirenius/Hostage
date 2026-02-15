using Hostage.SO;
using System.Collections.Generic;

namespace Hostage.Core
{
    [System.Flags]
    public enum  PersonFlag
    {
        None = 0,
        Available = 1 << 0, // is enabled in game for player to interact with
        Assistant = 1 << 1,
        Away = 1 << 2,
        Unknown = 1 << 3,
        Occupied = 1 << 4,
        // Add more statuses as needed
    }

    public class Person
    {
        private readonly SignalBus _signalBus;
        private readonly PersonManager _personManager;
        private PersonFlag _flag;

        public SOPerson SOReference { get; }

        public PersonFlag Flag
        {
            get => _flag;
            set
            {
                if (_flag == value) return;
                _flag = value;
                _signalBus.Publish(new PersonFlagsChangedSignal { Person = this });
            }
        }

        public PersonCommand Command { get; private set; }

        public List<SOIntel> Intels { get; } = new List<SOIntel>();

        public bool HasCompletedCommand(SOIntel intel, CommandType commandType)
            => _personManager.HasCompletedCommand(intel, commandType);

        public void RecordCompletedCommand(SOIntel intel, CommandType commandType)
            => _personManager.RecordCompletedCommand(intel, commandType);

        public Person(SOPerson soReference, SignalBus signalBus, PersonManager personManager)
        {
            _signalBus = signalBus;
            _personManager = personManager;
            SOReference = soReference;
            _flag = soReference.defaultFlag;
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
        
        public void AddIntel(SOIntel soIntel)
        {
            if (!Intels.Contains(soIntel))
                Intels.Add(soIntel);
        }

        public bool RemoveIntel(SOIntel soIntel) => Intels.Remove(soIntel);
        public bool HasIntel(SOIntel soIntel) => Intels.Contains(soIntel);

        public PersonCommand TryCreateCommand()
        {
            if (IsOccupied() || IsUnknown() || !IsAvailable())
                return null;

            Command = new PersonCommand(this);
            return Command;
        }

        public void ClearCommand()
        {
            Command = null;
        }

        public bool CanInteractWithIntel(SOIntel soIntel)
        {
            if (IsAssistant())
            {
                foreach (var verb in soIntel.GetAvailableVerbs())
                {
                    if (!HasCompletedCommand(soIntel, verb.CommandType))
                        return true;
                }
            }
            else
            {
                if (!HasCompletedCommand(soIntel, CommandType.None))
                    return true;
            }

            return false;
        }
    }
}