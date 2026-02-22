using System.Collections.Generic;
using UnityEngine;
using Hostage.Core;
using Hostage.SO;

namespace Hostage.Graphs
{
    
    
    public enum ContextKey
    {
        TimedEventIndex,
        VerbTypeIndex
    }

    public static class ContextKeyExtensions
    {
        public static string ToKeyString(this ContextKey key) => key switch
        {
            ContextKey.TimedEventIndex => GraphContext.TimedEventIndexKey,
            ContextKey.VerbTypeIndex => GraphContext.VerbTypeIndexKey,
            _ => throw new System.ArgumentOutOfRangeException(nameof(key), key, null)
        };
    }

    public class GraphContext
    {
        public const string TimedEventIndexKey = "timed_event_index";
        public const string VerbTypeIndexKey = "verb_type_index";

        public Person Person { get; }
        public SOIntel Intel { get; set; }
        public Dictionary<string, int> IntVariables { get; } = new();
        public Dictionary<string, string> StringVariables { get; } = new();

        public GraphResult Result { get; } = new();

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

        public bool HasIntelInStartNode(SOIntel soIntel)
        {
            if (Nodes == null || Nodes.Count == 0)
                return false;
            if (Nodes[0] is RTNpcStartNode startNode)
                return startNode.intelList.Contains(soIntel);
            return false;
        }
    }
}