#region Imports
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.GraphToolkit.Editor;
using UnityEditor.AssetImporters;
using UnityEngine;


#endregion

namespace Hostage.Graphs.Editor {
    [ScriptedImporter(1, EditorEventGraph.AssetExtension)]
    internal class EventGraphImporter : ScriptedImporter {
        public override void OnImportAsset(AssetImportContext ctx) {
            var graph = GraphDatabase.LoadGraphForImporter<EditorEventGraph>(ctx.assetPath);
            
            if (graph == null) {
                Debug.LogError($"Failed to load State Machine Director graph asset: {ctx.assetPath}");
                return;
            }
            
            var startNodeModel = graph.GetNodes().OfType<Graphs.Editor.EditorStartNode>().FirstOrDefault();
            
            if (startNodeModel == null) {
                return;
            }
            
            var runtimeAsset = ScriptableObject.CreateInstance<Graphs.EventGraph>();
            var nodeMap = new Dictionary<INode, int>();
            
            // First pass: Create all runtime nodes (without connections)
            CreateRuntimeNodes(startNodeModel, runtimeAsset, nodeMap);
            
            // Second pass: Set up connections using the indices
            SetupConnections(startNodeModel, runtimeAsset, nodeMap);
            
            ctx.AddObjectToAsset("RuntimeAsset", runtimeAsset);
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
                case Graphs.Editor.EditorStartNode:
                    returnedNodes.Add(new StartNode());
                    break;
                case Graphs.Editor.EditorDialogueNode dialogueNode:
                    var port = dialogueNode.GetInputPortByName("Dialogue");
                    var dialogueText = GetInputPortValue<string>(port);                    
                    //dialogueNode.GetNodeOptionByName("stateName")?.TryGetValue(out stateName);
                    returnedNodes.Add(
                        new DialogueNode()
                        {
                            dialogueText = dialogueText
                        });
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