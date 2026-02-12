using System.Collections.Generic;
using UnityEngine;

namespace Hostage.SO
{
    [CreateAssetMenu(fileName = "IntelList", menuName = "SO/IntelList", order = 0)]
    public class IntelList : ScriptableObject
    {
        [SerializeField]
        private List<SOIntel> _intels = new List<SOIntel>();

        public void SetIntels(List<SOIntel> newIntels)
        {
            _intels = newIntels;
        }

        public List<SOIntel> GetIntels() => _intels;
    }
}