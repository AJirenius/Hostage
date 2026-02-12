using System.Collections.Generic;
using VContainer;
using Hostage.Core;
using Hostage.SO;
using JetBrains.Annotations;
using UnityEngine;
using VContainer.Unity;

namespace Hostage.Graphs
{
    public class EventGraphRunner : IStartable
    {
        public EventGraph eventGraph;
        public GraphContext Context { get; private set; }

        // Cycle detection for value node evaluation
        private readonly HashSet<int> _evaluatingValueNodes = new();

        // Injected dependencies - accessible by nodes via runner reference
        [Inject] public PlayerInventory PlayerInventory { get; private set; }
        [Inject] public ActionManager ActionManager { get; private set; }
        [Inject] public PersonManager PersonManager { get; private set; }
        [Inject] public IntelProvider IntelProvider { get; private set; }

        [Inject]
        private void Init()
        {
            ActionManager.OnGraphRequested += RunGraph;
            Context = new GraphContext();
        }

        public void Start()
        {
            RunNode(0);
        }

        public void RunGraph(EventGraph graph, [CanBeNull] GraphContext context, int outputIndex)
        {
            // check if already got graph and if it hasn't finished yet
            if(eventGraph != null && eventGraph.Nodes.Count > 0)
            {
                Debug.LogWarning("EventGraphRunner is already running a graph. Overwriting with new graph.");
            }

            eventGraph = graph;
            Context = context ?? new GraphContext();

            if (outputIndex >= 0)
                RunStartNodeWithOutput(outputIndex);
            else
                RunNode(0);
        }

        public void RunGraph(EventGraph graph, GraphContext context = null)
        {
            RunGraph(graph, context, -1);
        }
        
        // Will run a startnode with multiple outputs, allowing you to choose which path to take right from the start
        public void RunStartNodeWithOutput(int outputIndex)
        {
            var node = eventGraph.Nodes[0];
            node.Execute(this, nextOutput => {
                if (node.nextNodeIndices.Count > outputIndex)
                    RunNode(node.nextNodeIndices[outputIndex]);
            });
        }
        
        public void RunNode(int index)
        {
            if (index < 0 || index >= eventGraph.Nodes.Count)
            {
                // graph is (probably) finished, or index is out of bounds
                eventGraph = null;
                return;
            };

            var node = eventGraph.Nodes[index];
            node.Execute(this, nextOutput => {
                if (node.nextNodeIndices.Count > nextOutput)
                    RunNode(node.nextNodeIndices[nextOutput]);
            });
        }

        public T ResolveDataPort<T>(DataPort port)
        {
            if (port.HasValueNode)
            {
                if (!_evaluatingValueNodes.Add(port.valueNodeIndex))
                {
                    Debug.LogError("Cycle detected evaluating value node at index " + port.valueNodeIndex);
                    return default;
                }

                try
                {
                    var valueNode = eventGraph.ValueNodes[port.valueNodeIndex];
                    var result = valueNode.Evaluate(this, port.outputPortIndex);
                    return (T)result;
                }
                finally
                {
                    _evaluatingValueNodes.Remove(port.valueNodeIndex);
                }
            }

            // Return baked value by type
            if (typeof(T) == typeof(string)) return (T)(object)port.stringValue;
            if (typeof(T) == typeof(int)) return (T)(object)port.intValue;
            if (typeof(T) == typeof(float)) return (T)(object)port.floatValue;
            if (typeof(T) == typeof(bool)) return (T)(object)port.boolValue;
            if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T))) return (T)(object)port.objectValue;

            Debug.LogWarning("ResolveDataPort: unsupported baked type " + typeof(T));
            return default;
        }
    }
}