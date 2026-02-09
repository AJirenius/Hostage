using System.Collections.Generic;
using UnityEngine;

namespace Hostage.SO
{
    [CreateAssetMenu(fileName = "ActionPersonList", menuName = "SO/ActionPersonList", order = 0)]
    public class ActionPersonList : ScriptableObject
    {
        [SerializeField]
        private List<ActionPerson> _persons = new List<ActionPerson>();

        public void SetPersons(List<ActionPerson> newPersons)
        {
            _persons = newPersons;
        }

        public List<ActionPerson> GetPersons() => _persons;
    }
}