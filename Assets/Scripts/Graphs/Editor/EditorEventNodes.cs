
using System;
using Hostage.SO;
using Unity.GraphToolkit.Editor;


namespace Hostage.Graphs.Editor
{
    [Serializable]
    public class EditorStartNode:Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddOutputPort("out").Build();
        }
    }
    
    [Serializable]
    public class EditorEndNode:Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("in").Build();
        }
    }
    
    [Serializable]
    public class EditorDialogueNode:Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("in").Build();
            context.AddOutputPort("out").Build();
            
            context.AddInputPort<Person>("Person").Build();
            context.AddInputPort<string>("Dialogue").Build();

        }
    }
    
    [Serializable]
    public class EditorGiveIntel:Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("in").Build();
            context.AddOutputPort("out").Build();
            
            context.AddInputPort<Intel>("Intel").Build();
            context.AddInputPort<Person>("Person").Build();

        }
    }
}