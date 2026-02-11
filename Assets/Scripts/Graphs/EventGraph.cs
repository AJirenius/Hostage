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

        public int StartNodeOutputCount;
        public List<int> ConnectedOutputs = new();

        public bool IsOutputConnected(int outputIndex) => ConnectedOutputs.Contains(outputIndex);
    }
}