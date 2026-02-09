using System.Collections.Generic;
using UnityEngine;

namespace Hostage.SO
{
    [CreateAssetMenu(fileName = "IntelList", menuName = "SO/IntelList", order = 0)]
    public class IntelList : ScriptableObject
    {
        [SerializeField]
        private List<Intel> _intels = new List<Intel>();

        public void SetIntels(List<Intel> newIntels)
        {
            _intels = newIntels;
        }

        public List<Intel> GetIntels() => _intels;
    }
}