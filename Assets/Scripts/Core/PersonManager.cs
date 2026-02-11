using System.Collections.Generic;
using Hostage.SO;

namespace Hostage.Core
{
    public class PersonManager
    {
        private readonly Dictionary<SOPerson, Person> _personMap = new();

        public PersonManager(SOPersonList personList)
        {
            LoadPersons(personList);
        }

        public void LoadPersons(SOPersonList personList)
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

        public Person GetPerson(SOPerson soPerson)
        {
            _personMap.TryGetValue(soPerson, out var person);
            return person;
        }
    }
}
