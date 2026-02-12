using System.Collections.Generic;
using UnityEngine;
using Hostage.Core;

namespace Hostage.Graphs
{
    public class GraphContext
    {
        public const string TimedEventIndexKey = "timed_event_index";

        public Person TriggeredBy { get; }
        public Dictionary<string, int> IntVariables { get; } = new();
        public Dictionary<string, string> StringVariables { get; } = new();

        public GraphContext(Person triggeredBy = null)
        {
            TriggeredBy = triggeredBy;
        }
    }

    public class EventGraph : ScriptableObject
    {
        [SerializeReference]
        public List<RuntimeNode> Nodes = new();

        [SerializeReference]
        public List<RuntimeValueNode> ValueNodes = new();

        public int StartNodeOutputCount;
        public List<int> ConnectedOutputs = new();

        public bool IsOutputConnected(int outputIndex) => ConnectedOutputs.Contains(outputIndex);
    }
}