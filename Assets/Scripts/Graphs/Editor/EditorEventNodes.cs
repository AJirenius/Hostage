
using System;
using Hostage.SO;
using Unity.GraphToolkit.Editor;


namespace Hostage.Graphs.Editor
{
    [Serializable]
    public class StartNode:Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddOutputPort("out").Build();
        }
    }
    
    [Serializable]
    public class EndNode:Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("in").Build();
        }
    }
    
    [Serializable]
    public class DialogueNode:Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("in").Build();
            context.AddOutputPort("out").Build();
            
            context.AddInputPort<SOPerson>("Person").Build();
            context.AddInputPort<string>("Dialogue").Build();

        }
    }
    
    [Serializable]
    public class GiveIntelToPerson:Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("in").Build();
            context.AddOutputPort("out").Build();
            
            context.AddInputPort<Intel>("Intel").Build();
            context.AddInputPort<SOPerson>("Person").Build();

        }
    }
    
    [Serializable]
    public class RemoveIntelFromPlayer:Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("in").Build();
            context.AddOutputPort("out").Build();
            
            context.AddInputPort<Intel>("Intel").Build();
        }
    }
    
    [Serializable]
    public class GiveIntelToPlayer:Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("in").Build();
            context.AddOutputPort("out").Build();
            
            context.AddInputPort<Intel>("Intel").Build();
        }
    }
    
    [Serializable]
    public class ReplaceIntelForPlayer:Node
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