using System;
using System.Collections.Generic;
using Hostage.Core;
using Hostage.Graphs;

namespace Hostage.SO
{
    
    [Serializable]
    public abstract class Verb
    {
        public bool isAvailable;
        public abstract CommandType CommandType { get; }

        public float baseTime; // seconds
        public List<TimeModifier> modifiers;
        public List<SkillTag> requiredTags;
        public bool occupyingIntel; // the intel will be unavailable during action
        public EventGraph result;
        public List<PersonStartMessage> personStartMessages;

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
        public override CommandType CommandType => CommandType.Investigate;
    }
    
    [Serializable]
    public class Interview: Verb
    {
        public override CommandType CommandType => CommandType.Interview;

        public List<SOIntel> linkedIntels; // intel that can be run instead of this
    }
    
    [Serializable]
    public class Surveillance: Verb
    {
        public override CommandType CommandType => CommandType.Surveillance;

        public List<SOIntel> linkedIntels; // intel that can be run instead of this
        public List<TimedEvent> timedEvents;
        
    }
    
    [Serializable]
    public class Analyze: Verb
    {
        public override CommandType CommandType => CommandType.Analyze;

        public List<TimedEvent> timedEvents;
    }
    
    [Serializable]
    public struct PersonStartMessage
    {
        public SOPerson person;
        public string message;
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
}