using Graphs;
using UnityEngine;

public class GraphRunner : MonoBehaviour
{
    public EventGraph eventGraph;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        int index = 0;
        while (eventGraph.Nodes[index] != null)
        {
            LogNode(index);
            if (eventGraph.Nodes[index].nextNodeIndices.Count > 0)
            {
                index = eventGraph.Nodes[index].nextNodeIndices[0];
            }
            else
            {
                break;
            }
        }
    }

    void LogNode(int index)
    {
        var node = eventGraph.Nodes[index];
        if (node is DialogueNode dialogueNode)
        {
            Debug.Log($"Dialogue Node: {dialogueNode.dialogueText}");
        }
        else if (node is StartNode)
        {
            Debug.Log("Start Node");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
