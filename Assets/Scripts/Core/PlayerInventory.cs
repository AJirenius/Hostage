using System.Collections.Generic;
using Hostage.SO;
using UnityEngine;

namespace Hostage.Core
{
    public class PlayerInventory
    {
        private readonly SignalBus _signalBus;
        private List<SOIntel> _intelList = new List<SOIntel>();

        public PlayerInventory(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }

        public bool AddIntel(SOIntel soIntel)
        {
            if (soIntel == null || _intelList.Contains(soIntel))
                return false;
            _intelList.Add(soIntel);
            Debug.Log($"Added Intel: {soIntel}");
            _signalBus.Publish(new IntelAddedSignal { SoIntel = soIntel });
            return true;
        }
        
        public bool HasIntel(SOIntel soIntel)
        {
            return _intelList.Contains(soIntel);
        }

        public bool RemoveIntel(SOIntel soIntel)
        {
            if (_intelList.Remove(soIntel))
            {
                _signalBus.Publish(new IntelRemovedSignal { SoIntel = soIntel });
                return true;
            }
            return false;
        }

        public IReadOnlyList<SOIntel> GetAllIntel()
        {
            return _intelList.AsReadOnly();
        }

        public List<SOIntel> GetIntelByCategory(IntelCategory category)
        {
            var result = new List<SOIntel>();
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