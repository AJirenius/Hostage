using System.Collections.Generic;
using UnityEngine;

namespace SO
{
    [CreateAssetMenu(fileName = "IntelList", menuName = "SO/IntelList", order = 0)]
    public class IntelList : ScriptableObject
    {
        List<Intel> intels;
    }
}