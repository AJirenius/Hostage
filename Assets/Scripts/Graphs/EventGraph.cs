using System;
using System.Collections.Generic;
using UnityEngine;
using Hostage.SO;

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
        public abstract void Execute(EventGraphRunner runner, Action<int> onComplete);
    }
    
    [Serializable]
    public class StartNode: RuntimeNode
    {
        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        {
            // Start node logic (if any)
            onComplete?.Invoke(0);
        }
    }
    
    [Serializable]
    public class EndNode : RuntimeNode
    {
        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        {
            // End node logic (if any)
            onComplete?.Invoke(0);
        }
    }
    
    [Serializable]
    public class DialogueNode: RuntimeNode
    {
       public string dialogueText;
       public Person speaker;
       
       public override void Execute(EventGraphRunner runner, Action<int> onComplete)
       {
           string name = speaker == null ? "You" : speaker.Name;
           Debug.Log(name + ": " + dialogueText);
           onComplete?.Invoke(0); // Replace with UI callback in real use
       }
    }

    [Serializable]
    public class GiveIntelToPersonNode : RuntimeNode
    {
        public Intel intel;
        public Person person;
        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        {
            // take intel from inventory and give to person
            
            Debug.Log("Taking " + intel.intelName + " from player and giving to " + person.Name);
            onComplete?.Invoke(0);
        }
    }

    [Serializable]
    public class RemoveIntelFromPlayerNode : RuntimeNode
    {
        public Intel intel;
        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        { 
            Debug.Log("Remodving " + intel.intelName + " from player inventory");
            if (runner.PlayerInventory.RemoveIntel(intel))           {
                Debug.Log("Successfully removed " + intel.intelName + " from player inventory");
            }
            else
            {
                Debug.LogWarning("Failed to remove " + intel.intelName + " from player inventory. Intel not found.");
            }
            onComplete?.Invoke(0);
        }
    }

    [Serializable]
    public class GiveIntelToPlayerNode : RuntimeNode
    {
        public Intel intel;
        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        {
            Debug.Log("Giving " + intel.intelName + " to player inventory");
            runner.PlayerInventory.AddIntel(intel);
            onComplete?.Invoke(0);
        }
    }

    [Serializable]
    public class ReplaceIntelForPlayerNode : RuntimeNode
    {
        public Intel oldIntel;
        public Intel newIntel;
        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        {
            runner.PlayerInventory.RemoveIntel(oldIntel);
            runner.PlayerInventory.AddIntel(newIntel);
            Debug.Log("Replacing " + oldIntel.intelName + " with " + newIntel.intelName + " in player inventory");
            onComplete?.Invoke(0);
        }
    }

}