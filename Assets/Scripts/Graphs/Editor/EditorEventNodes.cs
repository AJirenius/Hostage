
using System;
using Hostage.Core;
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
    public class ActionStartNode:Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddOutputPort("Investigate").Build();
            context.AddOutputPort("Interview").Build();
            context.AddOutputPort("Surveillance").Build();
            context.AddOutputPort("Analyze").Build();
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
        private const string TARGETNAME = "Target";
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<PersonTargetType>(TARGETNAME)
                .WithDisplayName("Target")
                .WithDefaultValue(PersonTargetType.SpecifiedPerson)
                .Delayed();
        }

        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            var option = GetNodeOptionByName(TARGETNAME);
            option.TryGetValue<PersonTargetType>(out var target);
            
            context.AddInputPort("in").Build();
            context.AddOutputPort("out").Build();
            if (target == PersonTargetType.SpecifiedPerson) context.AddInputPort<SOPerson>("Person").Build();
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
    
    [Serializable]
    public class SetPersonFlag:Node
    {
        const string k_TargetName = "Target";
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<PersonTargetType>(k_TargetName)
                .WithDisplayName("Target")
                .WithDefaultValue(PersonTargetType.SpecifiedPerson)
                .Delayed();
        }
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            var option = GetNodeOptionByName(k_TargetName);
            option.TryGetValue<PersonTargetType>(out var target);
            
            context.AddInputPort("in").Build();
            context.AddOutputPort("out").Build();
            
            context.AddInputPort<PersonFlag>("Flag").Build();
            if (target == PersonTargetType.SpecifiedPerson) context.AddInputPort<SOPerson>("Person").Build();
        }
    }
    
    [Serializable]
    public class ClearPersonFlag:Node
    {
        const string k_TargetName = "Target";
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<PersonTargetType>(k_TargetName)
                .WithDisplayName("Target")
                .WithDefaultValue(PersonTargetType.SpecifiedPerson)
                .Delayed();
        }
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            var option = GetNodeOptionByName(k_TargetName);
            option.TryGetValue<PersonTargetType>(out var target);
            
            context.AddInputPort("in").Build();
            context.AddOutputPort("out").Build();
            
            context.AddInputPort<PersonFlag>("Flag").Build();
            if (target == PersonTargetType.SpecifiedPerson) context.AddInputPort<SOPerson>("Person").Build();
        }
    }
    
    
}