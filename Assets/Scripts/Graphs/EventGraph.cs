using System.Collections.Generic;
using UnityEngine;
using Hostage.Core;

namespace Hostage.Graphs
{
    public class GraphContext
    {
        public Person TriggeredBy { get; }

        public GraphContext(Person triggeredBy = null)
        {
            TriggeredBy = triggeredBy;
        }
    }

    public class EventGraph : ScriptableObject
    {
        [SerializeReference]
        public List<RuntimeNode> Nodes = new();
    }
}