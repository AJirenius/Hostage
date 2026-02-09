using System.Collections.Generic;
using Hostage.SO;

namespace Hostage.Core
{
    public class IntelProvider
    {
        private readonly IntelList _intelList;
        private readonly List<Intel> _runtimeIntels;

        public IntelProvider(IntelList intelList)
        {
            _intelList = intelList;
            _runtimeIntels = new List<Intel>((IEnumerable<Intel>)intelList.GetIntels());
        }

        public IReadOnlyList<Intel> GetAllIntels() => _runtimeIntels;

        public Intel GetWithId(string id)
        {
            return _runtimeIntels.Find(i => i.name == id || i.intelName == id);
        }

        public void AddIntel(Intel intel)
        {
            if (!_runtimeIntels.Contains(intel))
                _runtimeIntels.Add(intel);
        }
    }
}
