using System;
using System.Collections.Generic;
using UnityEngine;
using Hostage.Core;
using Hostage.SO;
using UnityEngine.Serialization;

namespace Hostage.Graphs
{
    public enum PersonTargetType
    {
        SpecifiedPerson,
        Player,
        ContextPerson,
    }
    
    [Serializable]
    public abstract class RuntimeNode
    {
        public List<int> nextNodeIndices = new();
        public abstract void Execute(EventGraphRunner runner, Action<int> onComplete);
    }

    [Serializable]
    public class RTStartNode: RuntimeNode
    {
        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        {
            // Start node logic (if any)
            onComplete?.Invoke(0);
        }
    }

    [Serializable]
    public class RTEndNode : RuntimeNode
    {
        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        {
            // End node logic (if any)
            onComplete?.Invoke(0);
        }
    }

    [Serializable]
    public class RTDialogueNode: RuntimeNode
    {
       public string dialogueText;
       [FormerlySerializedAs("targetPerson")] public PersonTargetType personTargetType;
       public SOPerson speaker;

       public override void Execute(EventGraphRunner runner, Action<int> onComplete)
       {
           string name;
           switch (personTargetType)
           {
               case PersonTargetType.ContextPerson:
                   name = runner.Context.TriggeredBy != null ? runner.Context.TriggeredBy.SOReference.Name : "Someone";
                   break;
                case PersonTargetType.Player:
                   name = "You";
                   break;
                case PersonTargetType.SpecifiedPerson:
                    name = speaker.Name;
                   break;
               default:
                   throw new ArgumentOutOfRangeException();
           }

           Debug.Log(name + ": " + dialogueText);
           onComplete?.Invoke(0); // Replace with UI callback in real use
       }
    }

    [Serializable]
    public class RTGiveIntelToPersonNode : RuntimeNode
    {
        public Intel intel;
        public SOPerson soPerson;
        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        {
            // take intel from inventory and give to person

            Debug.Log("Taking " + intel.intelName + " from player and giving to " + soPerson.Name);
            onComplete?.Invoke(0);
        }
    }

    [Serializable]
    public class RTRemoveIntelFromPlayerNode : RuntimeNode
    {
        public Intel intel;
        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        {
            Debug.Log("Removing " + intel.intelName + " from player inventory");
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
    public class RTGiveIntelToPlayerNode : RuntimeNode
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
    public class RTReplaceIntelForPlayerNode : RuntimeNode
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

    [Serializable]
    public class RTSetPersonFlagNode : RuntimeNode
    {
        public PersonFlag flag;
        public PersonTargetType personTargetType;
        public SOPerson soPerson;

        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        {
            var person = ResolvePerson(runner);
            if (person != null)
            {
                person.Flag |= flag;
                Debug.Log("Set flag " + flag + " on " + person.SOReference.Name);
            }
            onComplete?.Invoke(0);
        }

        Person ResolvePerson(EventGraphRunner runner)
        {
            switch (personTargetType)
            {
                case PersonTargetType.ContextPerson:
                    return runner.Context.TriggeredBy;
                case PersonTargetType.SpecifiedPerson:
                    return runner.PersonManager.GetPerson(soPerson);
                default:
                    Debug.LogWarning("RTSetPersonFlagNode: unsupported target " + personTargetType);
                    return null;
            }
        }
    }

    [Serializable]
    public class RTClearPersonFlagNode : RuntimeNode
    {
        public PersonFlag flag;
        [FormerlySerializedAs("targetPerson")] public PersonTargetType personTargetType;
        public SOPerson soPerson;

        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        {
            var person = ResolvePerson(runner);
            if (person != null)
            {
                person.Flag &= ~flag;
                Debug.Log("Cleared flag " + flag + " on " + person.SOReference.Name);
            }
            onComplete?.Invoke(0);
        }

        Person ResolvePerson(EventGraphRunner runner)
        {
            switch (personTargetType)
            {
                case PersonTargetType.ContextPerson:
                    return runner.Context.TriggeredBy;
                case PersonTargetType.SpecifiedPerson:
                    return runner.PersonManager.GetPerson(soPerson);
                default:
                    Debug.LogWarning("RTClearPersonFlagNode: unsupported target " + personTargetType);
                    return null;
            }
        }
    }
}