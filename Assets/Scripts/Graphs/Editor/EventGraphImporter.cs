using System;
using System.Collections.Generic;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Hostage.Graphs.Editor {
    [ScriptedImporter(1, EditorEventGraph.AssetExtension)]
    internal class EventGraphImporter : ScriptedImporter {
        public override void OnImportAsset(AssetImportContext ctx) {
            var graph = GraphDatabase.LoadGraphForImporter<EditorEventGraph>(ctx.assetPath);
            
            if (graph == null) {
                Debug.LogError($"Failed to load State Machine Director graph asset: {ctx.assetPath}");
                return;
            }
            
            var startNodeModel = graph.GetNodes().OfType<Graphs.Editor.StartNode>().FirstOrDefault();
            
            if (startNodeModel == null) {
                return;
            }
            
            var runtimeAsset = ScriptableObject.CreateInstance<Graphs.EventGraph>();
            // Set the name so Unity can properly identify it in the inspector
            runtimeAsset.name = System.IO.Path.GetFileNameWithoutExtension(ctx.assetPath);
            
            var nodeMap = new Dictionary<INode, int>();
            
            // First pass: Create all runtime nodes (without connections)
            CreateRuntimeNodes(startNodeModel, runtimeAsset, nodeMap);
            
            // Second pass: Set up connections using the indices
            SetupConnections(startNodeModel, runtimeAsset, nodeMap);
            
            ctx.AddObjectToAsset("MainAsset", runtimeAsset);
            ctx.SetMainObject(runtimeAsset);
        }

        void CreateRuntimeNodes(INode startNode, Graphs.EventGraph graph, Dictionary<INode, int> nodeMap) {
            var nodesToProcess = new Queue<INode>();
            nodesToProcess.Enqueue(startNode);

            while (nodesToProcess.Count > 0) {
                var currentNode = nodesToProcess.Dequeue();
                
                if (nodeMap.ContainsKey(currentNode)) continue;
                
                var runtimeNodes = TranslateNodeModelToRuntimeNodes(currentNode);

                foreach (var runtimeNode in runtimeNodes) {
                    nodeMap[currentNode] = graph.Nodes.Count;
                    graph.Nodes.Add(runtimeNode);
                }
                
                // Queue up all connected nodes
                for (int i = 0; i < currentNode.outputPortCount; i++) {
                    var port = currentNode.GetOutputPort(i);

                    if (port.isConnected) {
                        nodesToProcess.Enqueue(port.firstConnectedPort.GetNode());
                    }
                }
            }
        }

        void SetupConnections(INode startNode, Graphs.EventGraph graph, Dictionary<INode, int> nodeMap) {
            foreach (var kvp in nodeMap) {
                var editorNode = kvp.Key;
                var runtimeIndex = kvp.Value;
                var runtimeNode = graph.Nodes[runtimeIndex];

                for (int i = 0; i < editorNode.outputPortCount; i++) {
                    var port = editorNode.GetOutputPort(i);

                    if (port.isConnected && nodeMap.TryGetValue(port.firstConnectedPort.GetNode(), out int nextIndex)) {
                        runtimeNode.nextNodeIndices.Add(nextIndex);
                    }
                }
            }
        }

        static List<RuntimeNode> TranslateNodeModelToRuntimeNodes(INode nodeModel) {
            var returnedNodes = new List<RuntimeNode>();

            switch (nodeModel) {
                case StartNode:
                    returnedNodes.Add(new RTStartNode());
                    break;
                case EndNode:
                    returnedNodes.Add(new RTEndNode());
                    break;
                case DialogueNode dialogueNode:
                    var dialoguePort = dialogueNode.GetInputPortByName("Dialogue");
                    var dialogueText = GetInputPortValue<string>(dialoguePort);
                    var targetPerson = dialogueNode.GetNodeOptionByName("Target").TryGetValue<PersonTargetType>(out var target) ? target : PersonTargetType.SpecifiedPerson;
                    var speaker = targetPerson == PersonTargetType.SpecifiedPerson?GetInputPortValue<Hostage.SO.SOPerson>(dialogueNode.GetInputPortByName("Person")):null;

                    returnedNodes.Add(
                        new RTDialogueNode()
                        {
                            speaker = speaker,
                            dialogueText = dialogueText,
                            personTargetType = targetPerson
                        });
                    break;
                case GiveIntelToPerson giveIntelNode:
                    var intelPort = giveIntelNode.GetInputPortByName("Intel");
                    var personPort = giveIntelNode.GetInputPortByName("Person");
                    var intel = GetInputPortValue<Hostage.SO.Intel>(intelPort);
                    var person = GetInputPortValue<Hostage.SO.SOPerson>(personPort);
                    returnedNodes.Add(new RTGiveIntelToPersonNode { intel = intel, soPerson = person });
                    break;
                case RemoveIntelFromPlayer removeIntelNode:
                    var removeIntelPort = removeIntelNode.GetInputPortByName("Intel");
                    var removeIntel = GetInputPortValue<Hostage.SO.Intel>(removeIntelPort);
                    returnedNodes.Add(new RTRemoveIntelFromPlayerNode { intel = removeIntel });
                    break;
                case GiveIntelToPlayer giveIntelToPlayerNode:
                    var giveIntelToPlayerPort = giveIntelToPlayerNode.GetInputPortByName("Intel");
                    var giveIntelToPlayer = GetInputPortValue<Hostage.SO.Intel>(giveIntelToPlayerPort);
                    returnedNodes.Add(new RTGiveIntelToPlayerNode { intel = giveIntelToPlayer });
                    break;
                case ReplaceIntelForPlayer replaceIntelNode:
                    var oldIntelPort = replaceIntelNode.GetInputPortByName("OldIntel");
                    var newIntelPort = replaceIntelNode.GetInputPortByName("NewIntel");
                    var oldIntel = GetInputPortValue<Hostage.SO.Intel>(oldIntelPort);
                    var newIntel = GetInputPortValue<Hostage.SO.Intel>(newIntelPort);
                    returnedNodes.Add(new RTReplaceIntelForPlayerNode { oldIntel = oldIntel, newIntel = newIntel });
                    break;
                case SetPersonFlag setFlagNode:
                    var setFlagTarget = setFlagNode.GetNodeOptionByName("Target").TryGetValue<PersonTargetType>(out var setTarget) ? setTarget : PersonTargetType.SpecifiedPerson;
                    var setFlagValue = GetInputPortValue<Hostage.Core.PersonFlag>(setFlagNode.GetInputPortByName("Flag"));
                    var setFlagPerson = setFlagTarget == PersonTargetType.SpecifiedPerson ? GetInputPortValue<Hostage.SO.SOPerson>(setFlagNode.GetInputPortByName("Person")) : null;
                    returnedNodes.Add(new RTSetPersonFlagNode { flag = setFlagValue, personTargetType = setFlagTarget, soPerson = setFlagPerson });
                    break;
                case ClearPersonFlag clearFlagNode:
                    var clearFlagTarget = clearFlagNode.GetNodeOptionByName("Target").TryGetValue<PersonTargetType>(out var clearTarget) ? clearTarget : PersonTargetType.SpecifiedPerson;
                    var clearFlagValue = GetInputPortValue<Hostage.Core.PersonFlag>(clearFlagNode.GetInputPortByName("Flag"));
                    var clearFlagPerson = clearFlagTarget == PersonTargetType.SpecifiedPerson ? GetInputPortValue<Hostage.SO.SOPerson>(clearFlagNode.GetInputPortByName("Person")) : null;
                    returnedNodes.Add(new RTClearPersonFlagNode { flag = clearFlagValue, personTargetType = clearFlagTarget, soPerson = clearFlagPerson });
                    break;
                default:
                    throw new ArgumentException($"Unsupported node type: {nodeModel.GetType()}");
            }
            
            return returnedNodes;
        }

        static T GetInputPortValue<T>(IPort port) {
            T value = default;

            if (port.isConnected) {
                switch (port.firstConnectedPort.GetNode()) {
                    case IVariableNode variableNode:
                        variableNode.variable.TryGetDefaultValue<T>(out value);
                        return value;
                    case IConstantNode constantNode:
                        constantNode.TryGetValue<T>(out value);
                        return value;
                    default:
                        break;
                }
            }
            else {
                port.TryGetValue(out value);
            }
            
            return value;
        }
    }
}