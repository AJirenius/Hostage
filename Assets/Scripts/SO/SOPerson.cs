using System.Collections.Generic;
using Hostage.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Hostage.SO
{
    [CreateAssetMenu(fileName = "SomePerson", menuName = "SO/Person", order = 0)]
    public class SOPerson : ScriptableObject
    {
        public string Name;
        public string FullName;
        public string Description;
        public Sprite Portrait;
        [FormerlySerializedAs("defaultStatus")] public PersonFlag defaultFlag;
        public List<SkillTag> skillTags;
    }
}
