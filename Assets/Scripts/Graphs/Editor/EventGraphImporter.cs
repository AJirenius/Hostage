using System;
using System.Collections.Generic;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace Hostage.Graphs.Editor {
    [ScriptedImporter(4, EditorEventGraph.AssetExtension)]
    internal class EventGraphImporter : ScriptedImporter {
        public override void OnImportAsset(AssetImportContext ctx) {
            var graph = GraphDatabase.LoadGraphForImporter<EditorEventGraph>(ctx.assetPath);

            if (graph == null) {
                Debug.LogError($"Failed to load State Machine Director graph asset: {ctx.assetPath}");
                return;
            }

            var startNodeModel = (INode)graph.GetNodes().OfType<Graphs.Editor.AssistantStartNode>().FirstOrDefault()
                              ?? (INode)graph.GetNodes().OfType<Graphs.Editor.NpcPersonStartNode>().FirstOrDefault()
                              ?? graph.GetNodes().OfType<Graphs.Editor.StartNode>().FirstOrDefault();

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

            // Third pass: Discover value nodes reachable from flow node data inputs
            var valueNodeMap = new Dictionary<INode, int>();
            DiscoverValueNodes(runtimeAsset, nodeMap, valueNodeMap);

            // Fourth pass: Wire DataPort references within value nodes
            WireValueNodeDataPorts(runtimeAsset, valueNodeMap);

            // Fifth pass: Wire DataPort references on flow nodes that use DataPort fields
            WireFlowNodeDataPorts(runtimeAsset, nodeMap, valueNodeMap);

            // Record start node output port metadata
            runtimeAsset.StartNodeOutputCount = startNodeModel.outputPortCount;
            for (int i = 0; i < startNodeModel.outputPortCount; i++) {
                if (startNodeModel.GetOutputPort(i).isConnected) {
                    runtimeAsset.ConnectedOutputs.Add(i);
                }
            }

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
                    } else {
                        runtimeNode.nextNodeIndices.Add(-1);
                    }
                }
            }
        }

        static List<RuntimeNode> TranslateNodeModelToRuntimeNodes(INode nodeModel) {
            var returnedNodes = new List<RuntimeNode>();
            IPort intelPort;
            switch (nodeModel) {
                case AssistantStartNode:
                    returnedNodes.Add(new RTAssistantStartNode());
                    break;
                case StartNode:
                    returnedNodes.Add(new RTStartNode());
                    break;
                case NpcPersonStartNode personStartNode:
                    var nrIntel = personStartNode.GetNodeOptionByName("NrIntel").TryGetValue<int>(out var intelCount) ? intelCount : 3;
                    var rtPersonStart = new RTPersonStartNode();
                    for (int i = 0; i < nrIntel; i++)
                    {
                        intelPort = personStartNode.GetInputPortByName($"intel{i}");
                        var soIntel = GetInputPortValue<Hostage.SO.SOIntel>(intelPort);
                        rtPersonStart.intelList.Add(soIntel);
                    }
                    returnedNodes.Add(rtPersonStart);
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
                    intelPort = giveIntelNode.GetInputPortByName("Intel");
                    var personPort = giveIntelNode.GetInputPortByName("Person");
                    var intel = GetInputPortValue<Hostage.SO.SOIntel>(intelPort);
                    var person = GetInputPortValue<Hostage.SO.SOPerson>(personPort);
                    returnedNodes.Add(new RTGiveIntelToPersonNode { soIntel = intel, soPerson = person });
                    break;
                case RemoveIntelFromPlayer removeIntelNode:
                    var removeIntelPort = removeIntelNode.GetInputPortByName("Intel");
                    var removeIntel = GetInputPortValue<Hostage.SO.SOIntel>(removeIntelPort);
                    returnedNodes.Add(new RTRemoveIntelFromPlayerNode { soIntel = removeIntel });
                    break;
                case GiveIntelToPlayer giveIntelToPlayerNode:
                    var giveIntelToPlayerPort = giveIntelToPlayerNode.GetInputPortByName("Intel");
                    var giveIntelToPlayer = GetInputPortValue<Hostage.SO.SOIntel>(giveIntelToPlayerPort);
                    returnedNodes.Add(new RTGiveIntelToPlayerNode { soIntel = giveIntelToPlayer });
                    break;
                case ReplaceIntelForPlayer replaceIntelNode:
                    var oldIntelPort = replaceIntelNode.GetInputPortByName("OldIntel");
                    var newIntelPort = replaceIntelNode.GetInputPortByName("NewIntel");
                    var oldIntel = GetInputPortValue<Hostage.SO.SOIntel>(oldIntelPort);
                    var newIntel = GetInputPortValue<Hostage.SO.SOIntel>(newIntelPort);
                    returnedNodes.Add(new RTReplaceIntelForPlayerNode { oldSoIntel = oldIntel, newSoIntel = newIntel });
                    break;
                case SetPersonFlag setFlagNode:
                    var setFlagTarget = setFlagNode.GetNodeOptionByName("Target").TryGetValue<PersonTargetType>(out var setTarget) ? setTarget : PersonTargetType.SpecifiedPerson;
                    var setFlagValue = GetInputPortValue<Hostage.Core.PersonFlag>(setFlagNode.GetInputPortByName("Flag"));
                    var setFlagBoolValue = GetInputPortValue<bool>(setFlagNode.GetInputPortByName("Value"));
                    var setFlagPerson = setFlagTarget == PersonTargetType.SpecifiedPerson ? GetInputPortValue<Hostage.SO.SOPerson>(setFlagNode.GetInputPortByName("Person")) : null;
                    returnedNodes.Add(new RTSetPersonFlagNode { flag = setFlagValue, value = setFlagBoolValue, personTargetType = setFlagTarget, soPerson = setFlagPerson });
                    break;
                case ClearScopeNode clearScopeNode:
                    var clearScopeValue = GetInputPortValue<Hostage.Core.FlagScope>(clearScopeNode.GetInputPortByName("Scope"));
                    returnedNodes.Add(new RTClearScopeNode { scope = clearScopeValue });
                    break;
                case IfNode:
                    returnedNodes.Add(new RTIfNode());
                    break;
                case SetFlag setFlagNode:
                    var setFlagValue2 = GetInputPortValue<Hostage.Core.Flag>(setFlagNode.GetInputPortByName("Flag"));
                    var setFlagBool = setFlagNode.GetNodeOptionByName("Value").TryGetValue<bool>(out var flagVal) ? flagVal : true;
                    returnedNodes.Add(new RTSetFlagNode { flag = setFlagValue2, value = setFlagBool });
                    break;
                case BranchByPerson branchByPersonNode:
                    var nrPersons = branchByPersonNode.GetNodeOptionByName("NrPersons").TryGetValue<int>(out var personCount) ? personCount : 3;
                    var rtBranchByPerson = new RTBranchByPersonNode();
                    for (int i = 0; i < nrPersons; i++)
                    {
                        var soPerson = GetInputPortValue<Hostage.SO.SOPerson>(branchByPersonNode.GetInputPortByName($"person{i}"));
                        rtBranchByPerson.personList.Add(soPerson);
                    }
                    returnedNodes.Add(rtBranchByPerson);
                    break;
                case GraphResultNode:
                    returnedNodes.Add(new RTGraphResultNode());
                    break;
                case BranchByIndex branchByIndexNode:
                    var branchSourceType = branchByIndexNode.GetNodeOptionByName("SourceType").TryGetValue<IndexSourceType>(out var srcType) ? srcType : IndexSourceType.Context;
                    var rtBranch = new RTBranchByIndexNode { sourceType = branchSourceType };
                    switch (branchSourceType)
                    {
                        case IndexSourceType.Context:
                            rtBranch.contextKeyEnum = GetInputPortValue<ContextKey>(branchByIndexNode.GetInputPortByName("ContextKey"));
                            break;
                        case IndexSourceType.CustomContext:
                            rtBranch.contextKey = new DataPort { valueNodeIndex = -1, stringValue = GetInputPortValue<string>(branchByIndexNode.GetInputPortByName("ContextKey")) };
                            break;
                        case IndexSourceType.GraphValue:
                            rtBranch.index = new DataPort { valueNodeIndex = -1, intValue = GetInputPortValue<int>(branchByIndexNode.GetInputPortByName("Index")) };
                            break;
                    }
                    returnedNodes.Add(rtBranch);
                    break;
                case DialogueChoiceNode dialogueChoiceNode:
                    var choiceDialogueText = GetInputPortValue<string>(dialogueChoiceNode.GetInputPortByName("Dialogue"));
                    var choiceTarget = dialogueChoiceNode.GetNodeOptionByName("Target").TryGetValue<PersonTargetType>(out var ct) ? ct : PersonTargetType.SpecifiedPerson;
                    var choiceSpeaker = choiceTarget == PersonTargetType.SpecifiedPerson
                        ? GetInputPortValue<Hostage.SO.SOPerson>(dialogueChoiceNode.GetInputPortByName("Person"))
                        : null;
                    var choiceNrOptions = dialogueChoiceNode.GetNodeOptionByName("NrOptions").TryGetValue<int>(out var nrOpts) ? nrOpts : 2;
                    var rtChoiceNode = new RTDialogueChoiceNode
                    {
                        dialogueText = choiceDialogueText,
                        personTargetType = choiceTarget,
                        speaker = choiceSpeaker
                    };
                    for (int i = 0; i < choiceNrOptions; i++)
                    {
                        var optText = GetInputPortValue<string>(dialogueChoiceNode.GetInputPortByName($"option{i}"));
                        rtChoiceNode.options.Add(optText);
                    }
                    returnedNodes.Add(rtChoiceNode);
                    break;
                default:
                    throw new ArgumentException($"Unsupported node type: {nodeModel.GetType()}");
            }
            
            return returnedNodes;
        }

        // ── Value Node Discovery & Wiring ─────────────────────────────────

        void DiscoverValueNodes(Graphs.EventGraph runtimeAsset, Dictionary<INode, int> flowNodeMap, Dictionary<INode, int> valueNodeMap) {
            // Walk data input ports of all discovered flow nodes.
            // If a data input connects to an IEditorValueNode, create the runtime value node.
            var nodesToVisit = new Queue<INode>();

            // Seed: scan all flow nodes for data inputs connected to value nodes
            foreach (var editorNode in flowNodeMap.Keys) {
                for (int i = 0; i < editorNode.inputPortCount; i++) {
                    var port = editorNode.GetInputPort(i);
                    if (port.isConnected) {
                        var connectedNode = port.firstConnectedPort.GetNode();
                        if (connectedNode is IEditorValueNode && !valueNodeMap.ContainsKey(connectedNode))
                            nodesToVisit.Enqueue(connectedNode);
                    }
                }
            }

            // BFS through value node chain (value nodes feeding into other value nodes)
            while (nodesToVisit.Count > 0) {
                var editorValueNode = nodesToVisit.Dequeue();
                if (valueNodeMap.ContainsKey(editorValueNode)) continue;

                var runtimeValueNode = TranslateEditorValueNode(editorValueNode);
                if (runtimeValueNode == null) {
                    Debug.LogWarning($"Unsupported value node type: {editorValueNode.GetType()}");
                    continue;
                }

                valueNodeMap[editorValueNode] = runtimeAsset.ValueNodes.Count;
                runtimeAsset.ValueNodes.Add(runtimeValueNode);

                // Check this value node's inputs for further value nodes
                for (int i = 0; i < editorValueNode.inputPortCount; i++) {
                    var port = editorValueNode.GetInputPort(i);
                    if (port.isConnected) {
                        var connectedNode = port.firstConnectedPort.GetNode();
                        if (connectedNode is IEditorValueNode && !valueNodeMap.ContainsKey(connectedNode))
                            nodesToVisit.Enqueue(connectedNode);
                    }
                }
            }
        }

        void WireValueNodeDataPorts(Graphs.EventGraph runtimeAsset, Dictionary<INode, int> valueNodeMap) {
            foreach (var kvp in valueNodeMap) {
                var editorNode = kvp.Key;
                var runtimeValueNode = runtimeAsset.ValueNodes[kvp.Value];

                switch (runtimeValueNode) {
                    case RTAddIntNode addNode:
                        addNode.a = BuildDataPort(editorNode.GetInputPortByName("A"), valueNodeMap);
                        addNode.b = BuildDataPort(editorNode.GetInputPortByName("B"), valueNodeMap);
                        break;
                    case RTContextIntNode ctxNode:
                        ctxNode.key = BuildDataPort(editorNode.GetInputPortByName("Key"), valueNodeMap);
                        break;
                    case RTRandomIntNode randNode:
                        randNode.min = BuildDataPort(editorNode.GetInputPortByName("Min"), valueNodeMap);
                        randNode.max = BuildDataPort(editorNode.GetInputPortByName("Max"), valueNodeMap);
                        break;
                    case RTGetFlagNode getFlagNode:
                        getFlagNode.flag = GetInputPortValue<Hostage.Core.Flag>(editorNode.GetInputPortByName("Flag"));
                        break;
                    case RTGetPersonFlagNode getPersonFlagNode:
                        switch (getPersonFlagNode.sourceType)
                        {
                            case PersonSourceType.GraphValue:
                                getPersonFlagNode.person = BuildDataPort(editorNode.GetInputPortByName("Person"), valueNodeMap);
                                break;
                            case PersonSourceType.CustomContext:
                                getPersonFlagNode.personKey = BuildDataPort(editorNode.GetInputPortByName("PersonKey"), valueNodeMap);
                                break;
                        }
                        break;
                }
            }
        }

        void WireFlowNodeDataPorts(Graphs.EventGraph runtimeAsset, Dictionary<INode, int> flowNodeMap, Dictionary<INode, int> valueNodeMap) {
            foreach (var kvp in flowNodeMap) {
                var editorNode = kvp.Key;
                var runtimeNode = runtimeAsset.Nodes[kvp.Value];

                switch (runtimeNode) {
                    case RTIfNode ifNode:
                        ifNode.condition = BuildDataPort(editorNode.GetInputPortByName("Condition"), valueNodeMap);
                        break;
                    case RTGraphResultNode graphResultNode:
                        graphResultNode.allowRepeat = BuildDataPort(editorNode.GetInputPortByName("AllowRepeat"), valueNodeMap);
                        graphResultNode.returnIntel = BuildDataPort(editorNode.GetInputPortByName("ReturnIntel"), valueNodeMap);
                        graphResultNode.scheduleNextIteration = BuildDataPort(editorNode.GetInputPortByName("ScheduleNextIteration"), valueNodeMap);
                        graphResultNode.nextIterationTime = BuildDataPort(editorNode.GetInputPortByName("NextIterationTime"), valueNodeMap);
                        graphResultNode.hideTime = BuildDataPort(editorNode.GetInputPortByName("HideTime"), valueNodeMap);
                        break;
                    case RTBranchByIndexNode branchNode:
                        switch (branchNode.sourceType) {
                            case IndexSourceType.Context:
                                // Enum-based: no DataPort wiring needed, value is baked as ContextKey enum
                                break;
                            case IndexSourceType.CustomContext:
                                branchNode.contextKey = BuildDataPort(editorNode.GetInputPortByName("ContextKey"), valueNodeMap);
                                break;
                            case IndexSourceType.GraphValue:
                                branchNode.index = BuildDataPort(editorNode.GetInputPortByName("Index"), valueNodeMap);
                                break;
                        }
                        break;
                }
            }
        }

        static RuntimeValueNode TranslateEditorValueNode(INode editorNode) {
            switch (editorNode) {
                case AddIntNode:
                    return new RTAddIntNode();
                case ContextIntNode:
                    return new RTContextIntNode();
                case RandomIntNode:
                    return new RTRandomIntNode();
                case GetFlag:
                    return new RTGetFlagNode();
                case GetPersonFlag getPersonFlagNode:
                    var personSourceType = getPersonFlagNode.GetNodeOptionByName("SourceType").TryGetValue<PersonSourceType>(out var pSrcType) ? pSrcType : PersonSourceType.ContextPerson;
                    return new RTGetPersonFlagNode
                    {
                        sourceType = personSourceType,
                        flag = GetInputPortValue<Hostage.Core.PersonFlag>(getPersonFlagNode.GetInputPortByName("Flag"))
                    };
                default:
                    return null;
            }
        }

        static DataPort BuildDataPort(IPort port, Dictionary<INode, int> valueNodeMap) {
            var dp = DataPort.Baked();

            if (port.isConnected) {
                var connectedNode = port.firstConnectedPort.GetNode();

                // Connected to a value node → reference it
                if (connectedNode is IEditorValueNode && valueNodeMap.TryGetValue(connectedNode, out int vnIndex)) {
                    // Find which output port index of the value node is connected
                    var connectedPort = port.firstConnectedPort;
                    int outputIndex = 0;
                    var sourceNode = connectedPort.GetNode();
                    for (int i = 0; i < sourceNode.outputPortCount; i++) {
                        if (sourceNode.GetOutputPort(i) == connectedPort) {
                            outputIndex = i;
                            break;
                        }
                    }
                    return DataPort.FromValueNode(vnIndex, outputIndex);
                }

                // Connected to constant/variable → bake value
                switch (connectedNode) {
                    case IVariableNode variableNode:
                        return BakeVariableToDataPort(variableNode);
                    case IConstantNode constantNode:
                        return BakeConstantToDataPort(constantNode);
                }
            }
            else {
                // Inline value on the port itself
                if (port.TryGetValue(out string s)) dp.stringValue = s;
                else if (port.TryGetValue(out int i)) dp.intValue = i;
                else if (port.TryGetValue(out float f)) dp.floatValue = f;
                else if (port.TryGetValue(out bool b)) dp.boolValue = b;
                else if (port.TryGetValue(out UnityEngine.Object o)) dp.objectValue = o;
            }

            return dp;
        }

        static DataPort BakeVariableToDataPort(IVariableNode variableNode) {
            var dp = DataPort.Baked();
            if (variableNode.variable.TryGetDefaultValue<string>(out var s)) dp.stringValue = s;
            else if (variableNode.variable.TryGetDefaultValue<int>(out var i)) dp.intValue = i;
            else if (variableNode.variable.TryGetDefaultValue<float>(out var f)) dp.floatValue = f;
            else if (variableNode.variable.TryGetDefaultValue<bool>(out var b)) dp.boolValue = b;
            else if (variableNode.variable.TryGetDefaultValue<UnityEngine.Object>(out var o)) dp.objectValue = o;
            return dp;
        }

        static DataPort BakeConstantToDataPort(IConstantNode constantNode) {
            var dp = DataPort.Baked();
            if (constantNode.TryGetValue<string>(out var s)) dp.stringValue = s;
            else if (constantNode.TryGetValue<int>(out var i)) dp.intValue = i;
            else if (constantNode.TryGetValue<float>(out var f)) dp.floatValue = f;
            else if (constantNode.TryGetValue<bool>(out var b)) dp.boolValue = b;
            else if (constantNode.TryGetValue<UnityEngine.Object>(out var o)) dp.objectValue = o;
            return dp;
        }

        // ── Existing Helpers ───────────────────────────────────────────────

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