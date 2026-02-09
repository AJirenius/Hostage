using System.Collections.Generic;
using Hostage.SO;
using UnityEngine;

namespace Hostage.Core
{
    public class PlayerInventory
    {
        private List<Intel> _intelList = new List<Intel>();

        public bool AddIntel(Intel intel)
        {
            if (intel == null || _intelList.Contains(intel))
                return false;
            _intelList.Add(intel);
            Debug.Log($"Added Intel: {intel}");
            return true;
        }

        public bool RemoveIntel(Intel intel)
        {
            return _intelList.Remove(intel);
        }

        public IReadOnlyList<Intel> GetAllIntel()
        {
            return _intelList.AsReadOnly();
        }

        public List<Intel> GetIntelByCategory(IntelCategory category)
        {
            var result = new List<Intel>();
            foreach (var intel in _intelList)
            {
                if (intel.category == category)
                {
                    result.Add(intel);
                }
            }
            return result;
        }
    }

   

    
}