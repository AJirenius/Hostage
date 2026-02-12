using System.Collections.Generic;
using UnityEngine;
using Hostage.Core;
using Hostage.SO;

namespace Hostage.Graphs
{
    public class GraphContext
    {
        public const string TimedEventIndexKey = "timed_event_index";
        public const string ActionOutputKey = "action_output";

        public Person Person { get; }
        public SOIntel Intel { get; set; }
        public Dictionary<string, int> IntVariables { get; } = new();
        public Dictionary<string, string> StringVariables { get; } = new();

        public GraphContext(Person person = null, SOIntel intel = null)
        {
            Person = person;
            Intel = intel;
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