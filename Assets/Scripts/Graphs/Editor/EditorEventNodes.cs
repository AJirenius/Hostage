
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
    public class AssistantStartNode:Node
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
    public class PersonStartNode:Node
    {
        const string NR_INTEL = "NrIntel";
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<int>(NR_INTEL)
                .WithDisplayName("Nr Intel")
                .WithDefaultValue(3)
                .Delayed();
        }
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            var option = GetNodeOptionByName(NR_INTEL);
            option.TryGetValue<int>(out var nrIntel);
            
            for (int i = 0; i < nrIntel; i++)
            {
                context.AddInputPort<SOIntel>($"intel{i}").Build();
                context.AddOutputPort($"out{i}").Build();
            }
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
            
            context.AddInputPort<SOIntel>("Intel").Build();
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
            
            context.AddInputPort<SOIntel>("Intel").Build();
        }
    }
    
    [Serializable]
    public class GiveIntelToPlayer:Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("in").Build();
            context.AddOutputPort("out").Build();
            
            context.AddInputPort<SOIntel>("Intel").Build();
        }
    }
    
    [Serializable]
    public class ReplaceIntelForPlayer:Node
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort("in").Build();
            context.AddOutputPort("out").Build();
            
            context.AddInputPort<SOIntel>("OldIntel").Build();
            context.AddInputPort<SOIntel>("NewIntel").Build();
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

    // ── Value Nodes (no flow ports) ───────────────────────────────────

    public interface IEditorValueNode { }

    [Serializable]
    public class AddIntNode : Node, IEditorValueNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<int>("A").Build();
            context.AddInputPort<int>("B").Build();
            context.AddOutputPort<int>("Result").Build();
        }
    }

    [Serializable]
    public class ContextIntNode : Node, IEditorValueNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<string>("Key").Build();
            context.AddOutputPort<int>("Value").Build();
        }
    }

    [Serializable]
    public class RandomIntNode : Node, IEditorValueNode
    {
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            context.AddInputPort<int>("Min").Build();
            context.AddInputPort<int>("Max").Build();
            context.AddOutputPort<int>("Result").Build();
        }
    }

    // ── Flow Nodes ──────────────────────────────────────────────────────

    [Serializable]
    public class BranchByIndex:Node
    {
        const string NR_OUTPUTS = "NrOutputs";
        const string SOURCE_TYPE = "SourceType";
        protected override void OnDefineOptions(IOptionDefinitionContext context)
        {
            context.AddOption<int>(NR_OUTPUTS)
                .WithDisplayName("Nr outputs")
                .WithDefaultValue(3)
                .Delayed();
            context.AddOption<IndexSourceType>(SOURCE_TYPE)
                .WithDisplayName("Source Type")
                .WithDefaultValue(IndexSourceType.Context)
                .Delayed();
        }
        protected override void OnDefinePorts(IPortDefinitionContext context)
        {
            var option = GetNodeOptionByName(NR_OUTPUTS);
            option.TryGetValue<int>(out var nrOutputs);
            var option2 = GetNodeOptionByName(SOURCE_TYPE);
            option2.TryGetValue<IndexSourceType>(out var sourceType);

            context.AddInputPort("in").Build();
            switch (sourceType)
            {
                case IndexSourceType.Context:
                    context.AddInputPort<string>("ContextKey").Build(); 
                    break;
                case IndexSourceType.GraphValue:
                    context.AddInputPort<int>("Index").Build(); 
                    break;
                default: 
                    throw new ArgumentOutOfRangeException();
            }

            for (int i = 0; i < nrOutputs; i++)
            {
                context.AddOutputPort($"out{i}").Build();
            }
        }
    }
    
    
}