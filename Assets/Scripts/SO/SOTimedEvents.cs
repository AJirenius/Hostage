using Hostage.Graphs;
using UnityEngine;

namespace Hostage.SO
{
    [CreateAssetMenu(fileName = "TimedEvents", menuName = "SO/TimedEvents", order = 0)]
    public class SOTimedEvents : ScriptableObject
    {
        public EventGraph eventGraph;
        public float initialTime;
    }
}
