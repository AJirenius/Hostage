using System.Collections.Generic;
using Hostage.Core;
using Hostage.Graphs;
using UnityEngine;
using UnityEngine.Serialization;

namespace Hostage.SO
{
    [CreateAssetMenu(fileName = "SomePerson", menuName = "SO/Person", order = 0)]
    public class SOPerson : ScriptableObject
    {
        public string Name;
        public string FullName;
        public string UnknownName;
        public string Description;
        public Sprite Portrait;
        [FormerlySerializedAs("defaultStatus")] public PersonFlag defaultFlag;
        public List<SkillTag> skillTags;
        public SOIntel personIntel;
        [FormerlySerializedAs("personMasterGraph")] public EventGraph npcGraph;
        public SOPersonDefaultMessages defaultIntelMessages;
    }
}