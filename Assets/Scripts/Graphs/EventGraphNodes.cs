using System;
using System.Collections.Generic;
using UnityEngine;
using Hostage.Core;
using Hostage.SO;
using UnityEngine.Serialization;

namespace Hostage.Graphs
{
    // ── Value Node System ──────────────────────────────────────────────

    [Serializable]
    public abstract class RuntimeValueNode
    {
        public abstract object Evaluate(EventGraphRunner runner, int outputIndex = 0);
    }

    [Serializable]
    public struct DataPort
    {
        public int valueNodeIndex;    // -1 = use baked value
        public int outputPortIndex;   // which output of the value node

        // Baked value storage (one field per supported type)
        public string stringValue;
        public int intValue;
        public float floatValue;
        public bool boolValue;
        public UnityEngine.Object objectValue; // covers Intel, SOPerson, etc.

        public bool HasValueNode => valueNodeIndex >= 0;

        public static DataPort Baked() => new DataPort { valueNodeIndex = -1 };

        public static DataPort FromValueNode(int nodeIndex, int outputPort = 0) =>
            new DataPort { valueNodeIndex = nodeIndex, outputPortIndex = outputPort };
    }

    [Serializable]
    public class RTAddIntNode : RuntimeValueNode
    {
        public DataPort a;
        public DataPort b;

        public override object Evaluate(EventGraphRunner runner, int outputIndex = 0)
        {
            var valA = runner.ResolveDataPort<int>(a);
            var valB = runner.ResolveDataPort<int>(b);
            return valA + valB;
        }
    }

    [Serializable]
    public class RTContextIntNode : RuntimeValueNode
    {
        public DataPort key;

        public override object Evaluate(EventGraphRunner runner, int outputIndex = 0)
        {
            var keyStr = runner.ResolveDataPort<string>(key);
            if (runner.Context.IntVariables.TryGetValue(keyStr, out var value))
                return value;

            Debug.LogWarning("RTContextIntNode: context key '" + keyStr + "' not found, returning 0");
            return 0;
        }
    }

    [Serializable]
    public class RTRandomIntNode : RuntimeValueNode
    {
        public DataPort min;
        public DataPort max;

        public override object Evaluate(EventGraphRunner runner, int outputIndex = 0)
        {
            var minVal = runner.ResolveDataPort<int>(min);
            var maxVal = runner.ResolveDataPort<int>(max);
            return UnityEngine.Random.Range(minVal, maxVal + 1);
        }
    }

    [Serializable]
    public class RTGetFlagNode : RuntimeValueNode
    {
        public Flag flag;

        public override object Evaluate(EventGraphRunner runner, int outputIndex = 0)
        {
            return runner.FlagManager.HasFlag(flag);
        }
    }

    [Serializable]
    public class RTGetPersonFlagNode : RuntimeValueNode
    {
        public PersonSourceType sourceType;
        public PersonFlag flag;
        public DataPort person;
        public DataPort personKey;

        public override object Evaluate(EventGraphRunner runner, int outputIndex = 0)
        {
            Person resolved = null;
            switch (sourceType)
            {
                case PersonSourceType.ContextPerson:
                    resolved = runner.Context.Person;
                    break;
                case PersonSourceType.GraphValue:
                    var soPerson = runner.ResolveDataPort<SOPerson>(person);
                    if (soPerson != null)
                        resolved = runner.PersonManager.GetPerson(soPerson);
                    break;
                case PersonSourceType.CustomContext:
                    var key = runner.ResolveDataPort<string>(personKey);
                    if (runner.Context.StringVariables.TryGetValue(key, out var personName))
                    {
                        foreach (var p in runner.PersonManager.GetAllPersons())
                        {
                            if (p.SOReference.Name == personName)
                            {
                                resolved = p;
                                break;
                            }
                        }
                    }
                    break;
            }

            if (resolved == null)
            {
                Debug.LogWarning("RTGetPersonFlagNode: could not resolve person");
                return false;
            }

            return (resolved.Flag & flag) != 0;
        }
    }

    // ── Flow Nodes ─────────────────────────────────────────────────────

    public enum PersonTargetType
    {
        SpecifiedPerson,
        Player,
        ContextPerson,
        Narrator
    }
    
    public enum PersonSourceType
    {
        ContextPerson,
        GraphValue,
        CustomContext
    }

    public enum IndexSourceType
    {
        Context,
        GraphValue,
        CustomContext
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
    public class RTPersonStartNode : RuntimeNode
    {
        public List<SOIntel> intelList = new();

        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        {
            if (runner.Context.Intel == null)
            {
                Debug.LogError("Context.Intel must be set for a PersonStartNode");
                return;
            }

            int index = intelList.FindIndex(intel => intel == runner.Context.Intel);
            index = index == -1?intelList.Count:index;
            onComplete?.Invoke(index);
        }
    }

    [Serializable]
    public class RTAssistantStartNode : RuntimeNode
    {
        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        {
            int output = 0;
            if (runner.Context.IntVariables.TryGetValue(GraphContext.VerbTypeIndexKey, out var value))
                output = value;
            onComplete?.Invoke(output);
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
                   name = runner.Context.Person != null ? runner.Context.Person.SOReference.Name : "Someone";
                   break;
                case PersonTargetType.Player:
                   name = "You";
                   break;
                case PersonTargetType.SpecifiedPerson:
                    name = speaker.Name;
                   break;
               case PersonTargetType.Narrator:
                   name = "Narrator";
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
        [FormerlySerializedAs("intel")] public SOIntel soIntel;
        public SOPerson soPerson;
        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        {
            Debug.Log("Taking " + soIntel.intelName + " from player and giving to " + soPerson.Name);

            var person = runner.PersonManager.GetPerson(soPerson);
            if (person != null)
            {
                if (runner.PlayerInventory.RemoveIntel(soIntel))
                {
                    person.AddIntel(soIntel);
                }
            }

            onComplete?.Invoke(0);
        }
    }

    [Serializable]
    public class RTRemoveIntelFromPlayerNode : RuntimeNode
    {
        [FormerlySerializedAs("intel")] public SOIntel soIntel;
        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        {
            Debug.Log("Removing " + soIntel.intelName + " from player inventory");
            if (runner.PlayerInventory.RemoveIntel(soIntel))           {
                Debug.Log("Successfully removed " + soIntel.intelName + " from player inventory");
            }
            else
            {
                Debug.LogWarning("Failed to remove " + soIntel.intelName + " from player inventory. Intel not found.");
            }
            onComplete?.Invoke(0);
        }
    }

    [Serializable]
    public class RTGiveIntelToPlayerNode : RuntimeNode
    {
        [FormerlySerializedAs("intel")] public SOIntel soIntel;
        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        {
            Debug.Log("Giving " + soIntel.intelName + " to player inventory");
            runner.PlayerInventory.AddIntel(soIntel);
            onComplete?.Invoke(0);
        }
    }

    [Serializable]
    public class RTReplaceIntelForPlayerNode : RuntimeNode
    {
        [FormerlySerializedAs("oldIntel")] public SOIntel oldSoIntel;
        [FormerlySerializedAs("newIntel")] public SOIntel newSoIntel;
        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        {
            runner.PlayerInventory.RemoveIntel(oldSoIntel);
            runner.PlayerInventory.AddIntel(newSoIntel);
            Debug.Log("Replacing " + oldSoIntel.intelName + " with " + newSoIntel.intelName + " in player inventory");
            onComplete?.Invoke(0);
        }
    }

    [Serializable]
    public class RTSetPersonFlagNode : RuntimeNode
    {
        public PersonFlag flag;
        public bool value;
        public PersonTargetType personTargetType;
        public SOPerson soPerson;

        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        {
            var person = ResolvePerson(runner);
            if (person != null)
            {
                if (value)
                    person.Flag |= flag;
                else
                    person.Flag &= ~flag;
                Debug.Log((value ? "Set" : "Cleared") + " flag " + flag + " on " + person.SOReference.Name);
            }
            onComplete?.Invoke(0);
        }

        Person ResolvePerson(EventGraphRunner runner)
        {
            switch (personTargetType)
            {
                case PersonTargetType.ContextPerson:
                    return runner.Context.Person;
                case PersonTargetType.SpecifiedPerson:
                    return runner.PersonManager.GetPerson(soPerson);
                default:
                    Debug.LogWarning("RTSetPersonFlagNode: unsupported target " + personTargetType);
                    return null;
            }
        }
    }

    [Serializable]
    public class RTIfNode : RuntimeNode
    {
        public DataPort condition;

        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        {
            var value = runner.ResolveDataPort<bool>(condition);
            onComplete?.Invoke(value ? 0 : 1);
        }
    }

    [Serializable]
    public class RTClearScopeNode : RuntimeNode
    {
        public FlagScope scope;

        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        {
            runner.FlagManager.ClearScope(scope);
            onComplete?.Invoke(0);
        }
    }

    [Serializable]
    public class RTSetFlagNode : RuntimeNode
    {
        public Flag flag;
        public bool value;

        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        {
            if (value)
                runner.FlagManager.SetFlag(flag);
            else
                runner.FlagManager.ClearFlag(flag);
            onComplete?.Invoke(0);
        }
    }

    [Serializable]
    public class RTBranchByPersonNode : RuntimeNode
    {
        public List<SOPerson> personList = new();

        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        {
            if (runner.Context.Person == null)
            {
                Debug.LogWarning("RTBranchByPersonNode: Context.Person is null, using default output");
                onComplete?.Invoke(0);
                return;
            }

            int index = personList.FindIndex(p => p == runner.Context.Person.SOReference);
            // output 0 = default, outputs 1..N = person matches
            int output = index == -1 ? 0 : index + 1;
            onComplete?.Invoke(output);
        }
    }

    [Serializable]
    public class RTAddTimedEventsNode : RuntimeNode
    {
        public SOTimedEvents soTimedEvents;

        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        {
            runner.CommandManager.AddTimedEvents(soTimedEvents);
            onComplete?.Invoke(0);
        }
    }

    [Serializable]
    public class RTBranchByIndexNode : RuntimeNode
    {
        public IndexSourceType sourceType;
        public ContextKey contextKeyEnum;
        public DataPort contextKey;
        public DataPort index;

        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        {
            int branchIndex;
            switch (sourceType)
            {
                case IndexSourceType.Context:
                    var enumKey = contextKeyEnum.ToKeyString();
                    if (runner.Context.IntVariables.TryGetValue(enumKey, out var enumValue))
                    {
                        branchIndex = enumValue;
                    }
                    else
                    {
                        Debug.LogWarning("RTBranchByIndexNode: context key '" + enumKey + "' not found, defaulting to 0");
                        branchIndex = 0;
                    }
                    break;
                case IndexSourceType.CustomContext:
                    var key = runner.ResolveDataPort<string>(contextKey);
                    if (runner.Context.IntVariables.TryGetValue(key, out var ctxValue))
                    {
                        branchIndex = ctxValue;
                    }
                    else
                    {
                        Debug.LogWarning("RTBranchByIndexNode: context key '" + key + "' not found, defaulting to 0");
                        branchIndex = 0;
                    }
                    break;
                case IndexSourceType.GraphValue:
                    branchIndex = runner.ResolveDataPort<int>(index);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            onComplete?.Invoke(branchIndex);
        }
    }

    [Serializable]
    public class RTGraphResultNode : RuntimeNode
    {
        public DataPort allowRepeat;
        public DataPort returnIntel;
        public DataPort scheduleNextIteration;
        public DataPort nextIterationTime;

        public override void Execute(EventGraphRunner runner, Action<int> onComplete)
        {
            var result = runner.Context.Result;
            result.AllowRepeat = runner.ResolveDataPort<bool>(allowRepeat);
            result.ReturnIntel = runner.ResolveDataPort<bool>(returnIntel);
            result.ScheduleNextIteration = runner.ResolveDataPort<bool>(scheduleNextIteration);
            result.NextIterationTime = runner.ResolveDataPort<float>(nextIterationTime);
            onComplete?.Invoke(0);
        }
    }
}