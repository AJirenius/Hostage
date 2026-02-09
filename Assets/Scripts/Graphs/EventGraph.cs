using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Hostage.Graphs
{
    public class EventGraph : ScriptableObject
    {
        [SerializeReference]
        public List<RuntimeNode> Nodes = new();
    }

    [Serializable]
    public abstract class RuntimeNode
    {
        public List<int> nextNodeIndices = new();
    }
    
    [Serializable]
    public class DialogueNode: RuntimeNode
    {
       public string dialogueText;
    }
    
    [Serializable]
    public class StartNode: RuntimeNode
    {
    }
}