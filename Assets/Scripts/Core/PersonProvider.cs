using System.Collections.Generic;

namespace Hostage.SO
{
    public class PersonProvider
    {
        private readonly ActionPersonList _personList;
        private readonly List<ActionPerson> _runtimePersons;

        public PersonProvider(ActionPersonList personList)
        {
            _personList = personList;
            _runtimePersons = new List<ActionPerson>((IEnumerable<ActionPerson>)personList.GetPersons());
        }

        public IReadOnlyList<ActionPerson> GetAllPersons() => _runtimePersons;

        public ActionPerson GetWithId(string id)
        {
            return _runtimePersons.Find(p => p.name == id || p.Name == id);
        }

        public void AddPerson(ActionPerson person)
        {
            if (!_runtimePersons.Contains(person))
                _runtimePersons.Add(person);
        }
    }
}
