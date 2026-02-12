using System.Collections.Generic;
using Hostage.SO;

namespace Hostage.Core
{
    public class IntelProvider
    {
        private readonly IntelList _intelList;
        private readonly List<SOIntel> _runtimeIntels;

        public IntelProvider(IntelList intelList)
        {
            _intelList = intelList;
            _runtimeIntels = new List<SOIntel>((IEnumerable<SOIntel>)intelList.GetIntels());
        }

        public IReadOnlyList<SOIntel> GetAllIntels() => _runtimeIntels;

        public SOIntel GetWithId(string id)
        {
            return _runtimeIntels.Find(i => i.name == id || i.intelName == id);
        }

        public void AddIntel(SOIntel soIntel)
        {
            if (!_runtimeIntels.Contains(soIntel))
                _runtimeIntels.Add(soIntel);
        }
    }
}
