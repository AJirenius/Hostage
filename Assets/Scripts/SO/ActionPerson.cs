using System.Collections.Generic;
using UnityEngine;

namespace Hostage.SO
{
    [CreateAssetMenu(fileName = "SomeActionPerson", menuName = "SO/ActionPerson", order = 0)]
    public class ActionPerson : Person
    {
        public List<SkillTag> skillTags;
        public string actionDescription;
    }
}
