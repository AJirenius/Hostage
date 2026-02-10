using System.Collections.Generic;
using UnityEngine;

namespace Hostage.SO
{
    [CreateAssetMenu(fileName = "ActionPersonList", menuName = "SO/ActionPersonList", order = 0)]
    public class SOActionPersonList : ScriptableObject
    {
        [SerializeField]
        private List<SOActionPerson> _persons = new List<SOActionPerson>();

        public void SetPersons(List<SOActionPerson> newPersons)
        {
            _persons = newPersons;
        }

        public List<SOActionPerson> GetPersons() => _persons;
    }
}