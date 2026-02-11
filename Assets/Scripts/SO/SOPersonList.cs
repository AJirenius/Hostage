using System.Collections.Generic;
using UnityEngine;

namespace Hostage.SO
{
    [CreateAssetMenu(fileName = "PersonList", menuName = "SO/PersonList", order = 0)]
    public class SOPersonList : ScriptableObject
    {
        [SerializeField]
        private List<SOPerson> _persons = new List<SOPerson>();

        public void SetPersons(List<SOPerson> newPersons)
        {
            _persons = newPersons;
        }

        public List<SOPerson> GetPersons() => _persons;
    }
}
