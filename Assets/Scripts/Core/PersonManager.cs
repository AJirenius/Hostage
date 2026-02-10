using System.Collections.Generic;
using Hostage.SO;

namespace Hostage.Core
{
    public class PersonManager
    {
        private readonly Dictionary<SOActionPerson, Person> _personMap = new();

        public PersonManager(SOActionPersonList personList)
        {
            LoadPersons(personList);
        }

        public void LoadPersons(SOActionPersonList personList)
        {
            _personMap.Clear();
            foreach (var soPerson in personList.GetPersons())
            {
                if (soPerson != null && !_personMap.ContainsKey(soPerson))
                    _personMap[soPerson] = new Person(soPerson);
            }
        }

        public void ClearPersons()
        {
            _personMap.Clear();
        }

        public IReadOnlyCollection<Person> GetAllPersons() => _personMap.Values;

        public Person GetPerson(SOActionPerson soPerson)
        {
            _personMap.TryGetValue(soPerson, out var person);
            return person;
        }

        public Person GetPersonById(string id)
        {
            foreach (var kvp in _personMap)
            {
                if (kvp.Key.name == id || kvp.Key.Name == id)
                    return kvp.Value;
            }
            return null;
        }
    }
}
