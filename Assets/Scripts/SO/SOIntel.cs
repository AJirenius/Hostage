using System;
using System.Collections.Generic;
using Hostage.Core;
using Hostage.Graphs;
using UnityEngine;

namespace Hostage.SO
{
    [CreateAssetMenu(fileName = "NewIntel", menuName = "SO/Intel", order = 0)]
    public class SOIntel : ScriptableObject
    {
        public string intelName;
        public string description; // this should change due to what intel one have.
        public IntelCategory category = IntelCategory.Unknown;

        public EventGraph masterGraph;

        public Investigate investigate;
        public Interview interview;
        public Surveillance surveillance;
        public Analyze analyze;
        
        public SOPerson person;
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
        public EventGraph result;

        public float GetModifier(SOPerson soPerson)
        {
            
            float modifier = 1;
            foreach ( TimeModifier mod in modifiers )
            {
                if (soPerson.skillTags.Contains(mod.skillTag))
                {
                    modifier += mod.value;
                }
            }
            return modifier; 
        }
        
        public float GetModifiedTime(SOPerson soPerson)
        {
            return baseTime * GetModifier(soPerson);
        }
    }
    
    // make a scriptable object that is a list of intel
    

    [Serializable]
    public class Investigate: Verb
    {
        public override ActionType actionType => ActionType.Investigate;
    }
    
    [Serializable]
    public class Interview: Verb
    {
        public override ActionType actionType => ActionType.Interview;

        public List<SOIntel> linkedIntels; // intel that can be run instead of this
    }
    
    [Serializable]
    public class Surveillance: Verb
    {
        public override ActionType actionType => ActionType.Surveillance;

        public List<SOIntel> linkedIntels; // intel that can be run instead of this
        public List<TimedEvent> timedEvents;
        
    }
    
    [Serializable]
    public class Analyze: Verb
    {
        public override ActionType actionType => ActionType.Analyze;

        public List<TimedEvent> timedEvents;
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