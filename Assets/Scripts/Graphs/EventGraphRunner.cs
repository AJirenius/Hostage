using VContainer;
using Hostage.Core;
using Hostage.SO;
using UnityEngine;
using VContainer.Unity;

namespace Hostage.Graphs
{
    public class EventGraphRunner : IStartable
    {
        public EventGraph eventGraph;
        public GraphContext Context { get; private set; }

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

        public void RunGraph(EventGraph graph, GraphContext context, int outputIndex)
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
    }
}