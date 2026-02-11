using System.Collections.Generic;
using Hostage.SO;
using UnityEngine;

namespace Hostage.Core
{
    public class PlayerInventory
    {
        private readonly SignalBus _signalBus;
        private List<Intel> _intelList = new List<Intel>();

        public PlayerInventory(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public bool AddIntel(Intel intel)
        {
            if (intel == null || _intelList.Contains(intel))
                return false;
            _intelList.Add(intel);
            Debug.Log($"Added Intel: {intel}");
            _signalBus.Publish(new IntelAddedSignal { Intel = intel });
            return true;
        }
        
        public bool HasIntel(Intel intel)
        {
            return _intelList.Contains(intel);
        }

        public bool RemoveIntel(Intel intel)
        {
            if (_intelList.Remove(intel))
            {
                _signalBus.Publish(new IntelRemovedSignal { Intel = intel });
                return true;
            }
            return false;
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