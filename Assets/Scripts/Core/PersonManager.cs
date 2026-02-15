using System.Collections.Generic;
using Hostage.SO;

namespace Hostage.Core
{
    public class PersonManager
    {
        private readonly Dictionary<SOPerson, Person> _personMap = new();
        private readonly HashSet<(SOIntel, CommandType)> _globalCompletedCommands = new();
        private readonly SignalBus _signalBus;

        public PersonManager(SOPersonList personList, SignalBus signalBus)
        {
            _signalBus = signalBus;
            LoadPersons(personList);
        }

        public void LoadPersons(SOPersonList personList)
        {
            _personMap.Clear();
            foreach (var soPerson in personList.GetPersons())
            {
                if (soPerson != null && !_personMap.ContainsKey(soPerson))
                    _personMap[soPerson] = new Person(soPerson, _signalBus, this);
            }
        }

        public void ClearPersons()
        {
            _personMap.Clear();
        }

        public IReadOnlyCollection<Person> GetAllPersons() => _personMap.Values;

        public Person GetPerson(SOPerson soPerson)
        {
            _personMap.TryGetValue(soPerson, out var person);
            return person;
        }

        public bool HasCompletedCommand(SOIntel intel, CommandType commandType)
            => _globalCompletedCommands.Contains((intel, commandType));

        public void RecordCompletedCommand(SOIntel intel, CommandType commandType)
            => _globalCompletedCommands.Add((intel, commandType));
    }
}
