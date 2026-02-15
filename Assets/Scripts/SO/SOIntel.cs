using System;
using System.Collections.Generic;
using Hostage.Core;
using Hostage.Graphs;
using UnityEngine;
using UnityEngine.Serialization;

namespace Hostage.SO
{
    [CreateAssetMenu(fileName = "NewIntel", menuName = "SO/Intel", order = 0)]
    public class SOIntel : ScriptableObject
    {
        public string intelName;
        public string description; // this should change due to what intel one have.
        public IntelCategory category = IntelCategory.Unknown;

        [FormerlySerializedAs("masterGraph")] public EventGraph graph;

        public Investigate investigate;
        public Interview interview;
        public Surveillance surveillance;
        public Analyze analyze;
        
        public SOPerson person;
        
        public List<Verb> GetAvailableVerbs()
        {
            var verbs = new List<Verb>();
            if (investigate is { isAvailable: true }) verbs.Add(investigate);
            if (interview is { isAvailable: true }) verbs.Add(interview);
            if (surveillance is { isAvailable: true }) verbs.Add(surveillance);
            if (analyze is { isAvailable: true }) verbs.Add(analyze);
            return verbs;
        }
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