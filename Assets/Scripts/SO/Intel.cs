using System;
using System.Collections.Generic;
using Hostage.Core;
using Hostage.Graphs;
using UnityEngine;

namespace Hostage.SO
{
    [CreateAssetMenu(fileName = "NewIntel", menuName = "SO/Intel", order = 0)]
    public class Intel : ScriptableObject
    {
        public string intelName;
        public string description; // this should change due to what intel one have.
        public IntelCategory category = IntelCategory.Unknown;

        public Investigate investigate;
        public Interview interview;
        public Surveillance surveillance;
        public Analyze analyze;
    }
    
    [Serializable]
    public abstract class Verb
    {
        public bool isAvailable;
        public abstract ActionType actionType { get; }

        public float baseTime; // seconds
        public List<TimeModifier> modifiers;
        public List<SkillTag> requiredTags;
        public bool occupyingIntel; // the intel will be unavailable during action

        public float GetModifier(ActionPerson person)
        {
            
            float modifier = 1;
            foreach ( TimeModifier mod in modifiers )
            {
                if (person.skillTags.Contains(mod.skillTag))
                {
                    modifier += mod.value;
                }
            }
            return modifier; 
        }
        
        public float GetModifiedTime(ActionPerson person)
        {
            return baseTime * GetModifier(person);
        }
    }
    
    // make a scriptable object that is a list of intel
    

    [Serializable]
    public class Investigate: Verb
    {
        public override ActionType actionType => ActionType.Investigate;

        public EventGraph result;
       
    }
    
    [Serializable]
    public class Interview: Verb
    {
        public override ActionType actionType => ActionType.Interview;

        public List<Intel> linkedIntels; // intel that can be run instead of this
        public EventGraph result;
       
    }
    
    [Serializable]
    public class Surveillance: Verb
    {
        public override ActionType actionType => ActionType.Surveillance;

        public List<Intel> linkedIntels; // intel that can be run instead of this
        public List<TimedEvent> timedEvents;
        
    }
    
    [Serializable]
    public class Analyze: Verb
    {
        public override ActionType actionType => ActionType.Analyze;

        public List<TimedEvent> timedEvents;
        public EventGraph result;
        
    }

    [Serializable]
    public class TimeModifier
    {
        public SkillTag skillTag;
        public float value;
    }
    
    
    
    [Serializable]
    public class TimedEvent
    {
        public int time;
        public EventGraph graph;
    }
    
    

    public enum SkillTag
    {
        Social, 
        Tech,
        NoDrivingLicense,
        Forensics,
    }
    
    // Enum for Intel categories (expand as needed)
    public enum IntelCategory
    {
        Unknown,
        PhoneNumber,
        Person,
        Location,
        Document,
        // Add more as needed
    }
}