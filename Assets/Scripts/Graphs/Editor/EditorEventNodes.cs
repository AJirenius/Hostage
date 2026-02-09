
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
    public class EditorGiveIntelToPerson:Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("in").Build();
            context.AddOutputPort("out").Build();
            
            context.AddInputPort<Intel>("Intel").Build();
            context.AddInputPort<Person>("Person").Build();

        }
    }
    
    [Serializable]
    public class EditorRemoveIntelFromPlayer:Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("in").Build();
            context.AddOutputPort("out").Build();
            
            context.AddInputPort<Intel>("Intel").Build();
        }
    }
    
    [Serializable]
    public class EditorGiveIntelToPlayer:Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("in").Build();
            context.AddOutputPort("out").Build();
            
            context.AddInputPort<Intel>("Intel").Build();
        }
    }
    
    [Serializable]
    public class EditorReplaceIntelForPlayer:Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("in").Build();
            context.AddOutputPort("out").Build();
            
            context.AddInputPort<Intel>("OldIntel").Build();
            context.AddInputPort<Intel>("NewIntel").Build();
        }
    }
}