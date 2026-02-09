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

        // Injected dependencies - accessible by nodes via runner reference
        [Inject] public PlayerInventory PlayerInventory { get; private set; }
        [Inject] public ActionManager ActionManager { get; private set; }
        [Inject] public PersonProvider PersonProvider { get; private set; }
        [Inject] public IntelProvider IntelProvider { get; private set; }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void Start()
        {
            RunNode(0);
        }

        public void RunGraph(EventGraph graph)
        {
            // check if already got graph and if it hasn't finished yet
            if(eventGraph != null && eventGraph.Nodes.Count > 0)
            {
                Debug.LogWarning("EventGraphRunner is already running a graph. Overwriting with new graph.");
            }
            
            eventGraph = graph;
            RunNode(0);
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