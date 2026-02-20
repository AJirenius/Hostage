using System;
using System.Collections.Generic;
using VContainer;
using Hostage.Core;
using Hostage.SO;
using UnityEngine;
using VContainer.Unity;

namespace Hostage.Graphs
{
    public class EventGraphRunner
    {
        public EventGraph eventGraph;
        public GraphContext Context { get; private set; }

        // Cycle detection for value node evaluation
        private readonly HashSet<int> _evaluatingValueNodes = new();
        private Action<GraphResult> _onGraphCompleted;

        // Injected dependencies - accessible by nodes via runner reference
        [Inject] public PlayerInventory PlayerInventory { get; private set; }
        [Inject] public CommandManager CommandManager { get; private set; }
        [Inject] public PersonManager PersonManager { get; private set; }
        [Inject] public IntelProvider IntelProvider { get; private set; }
        [Inject] public FlagManager FlagManager { get; private set; }
        [Inject] public SignalBus SignalBus { get; private set; }

        [Inject]
        private void Init()
        {
            CommandManager.OnGraphRequested += RunGraph;
        }

        public void RunGraph(EventGraph graph, GraphContext context=null, Action<GraphResult> onCompleted=null)
        {
            if (eventGraph != null && eventGraph.Nodes.Count > 0)
                Debug.LogWarning("EventGraphRunner is already running a graph. Overwriting with new graph.");

            eventGraph = graph;
            Context = context ?? new GraphContext();
            _onGraphCompleted = onCompleted ?? (result => { });
            SignalBus.Publish(new Core.GraphStartedSignal());
            RunNode(0);
        }

        public void RunNode(int index)
        {
            if (index < 0 || index >= eventGraph.Nodes.Count)
            {
                // graph is finished, or index is out of bounds
                var completed = _onGraphCompleted;
                _onGraphCompleted = null;
                eventGraph = null;
                SignalBus.Publish(new Core.GraphCompletedSignal());
                completed?.Invoke(Context.Result);
                return;
            };

            var node = eventGraph.Nodes[index];
            node.Execute(this, nextOutput => {
                if (node.nextNodeIndices.Count > nextOutput)
                    RunNode(node.nextNodeIndices[nextOutput]);
                else
                    RunNode(-1);
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