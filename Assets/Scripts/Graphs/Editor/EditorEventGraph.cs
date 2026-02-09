using System;
using UnityEngine;
using Unity.GraphToolkit.Editor;
using UnityEditor;

namespace Hostage.Graphs.Editor
{
    [Serializable]
    [Graph(AssetExtension)]

    public class EditorEventGraph : Graph
    {
        public const string AssetExtension = "eventgraph";

        [MenuItem("Assets/Create/EventGraph", false)]
        private static void CreateAssetFile()
        {
            GraphDatabase.PromptInProjectBrowserToCreateNewAsset<EditorEventGraph>("EventGraphNew");
        }
    }
}