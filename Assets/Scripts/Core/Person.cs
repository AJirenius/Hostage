using Hostage.SO;
using System.Collections.Generic;

namespace Hostage.Core
{
    [System.Flags]
    public enum  PersonFlag
    {
        None = 0,
        Available = 1 << 0, // visible in UI and interactable
        Assistant = 1 << 1,
        Away = 1 << 2,
        Identified = 1 << 3, // cosmetic: shows full name/details
        Occupied = 1 << 4,
        Known = 1 << 5, // player has access to this person's intel
    }

    public class Person
    {
        private readonly SignalBus _signalBus;
        private readonly PersonManager _personManager;
        private readonly PlayerInventory _playerInventory;
        private PersonFlag _flag;

        public SOPerson SOReference { get; }

        public PersonFlag Flag
        {
            get => _flag;
            set
            {
                if (_flag == value) return;

                _flag = value;

                // Known means player gets access to this person's intel
                if ((_flag & PersonFlag.Known) != 0 && SOReference.personIntel != null)
                    _playerInventory.AddIntel(SOReference.personIntel);

                _signalBus.Publish(new PersonFlagsChangedSignal { Person = this });
            }
        }

        public PersonCommand Command { get; private set; }

        public List<SOIntel> Intels { get; } = new List<SOIntel>();

        public bool HasCompletedCommand(SOIntel intel, CommandType commandType)
            => _personManager.HasCompletedCommand(intel, commandType);

        public void RecordCompletedCommand(SOIntel intel, CommandType commandType)
            => _personManager.RecordCompletedCommand(intel, commandType);

        public Person(SOPerson soReference, SignalBus signalBus, PersonManager personManager, PlayerInventory playerInventory)
        {
            _signalBus = signalBus;
            _personManager = personManager;
            _playerInventory = playerInventory;
            SOReference = soReference;
            _flag = soReference.defaultFlag;
        }

        // helpers
        public bool IsAvailable() => (Flag & PersonFlag.Available) != 0;
        public bool IsAway() => (Flag & PersonFlag.Away) != 0;
        public bool IsIdentified() => (Flag & PersonFlag.Identified) != 0;
        public bool IsOccupied() => (Flag & PersonFlag.Occupied) != 0;
        public bool IsAssistant() => (Flag & PersonFlag.Assistant) != 0;
        
        public void SetAway() => Flag |= PersonFlag.Away;
        public void SetIdentified() => Flag |= PersonFlag.Identified;
        public void SetOccupied() => Flag |= PersonFlag.Occupied;
        public void SetAvailable() => Flag |= PersonFlag.Available;
        public void SetAssistant() => Flag |= PersonFlag.Assistant;
        
        public void ClearAway() => Flag &= ~PersonFlag.Away;
        public void ClearIdentified() => Flag &= ~PersonFlag.Identified;
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
            if (IsOccupied() || !IsAvailable())
                return null;

            Command = new PersonCommand(this);
            return Command;
        }

        public void ClearCommand()
        {
            Command = null;
        }

        public bool HasReadyCommand() => Command != null && Command.readyToExecute;

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
                var graph = SOReference.personMasterGraph;
                if (graph == null || !graph.HasIntelInStartNode(soIntel))
                    return false;

                if (!HasCompletedCommand(soIntel, CommandType.None))
                    return true;
            }

            return false;
        }
    }
}