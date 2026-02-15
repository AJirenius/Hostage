using System.Collections.Generic;
using Hostage.Core;
using Hostage.Graphs;
using UnityEngine;
using UnityEngine.Serialization;

namespace Hostage.SO
{
    [CreateAssetMenu(fileName = "TimedEvents", menuName = "SO/TimedEvents", order = 0)]
    public class SOTimedEvents : ScriptableObject
    {
        public EventGraph eventGraph;
        public List<TimedEvent> timedEvents;
    }
}
