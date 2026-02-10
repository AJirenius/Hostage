using System.Collections.Generic;
using Hostage.Core;
using UnityEngine;

namespace Hostage.SO
{
    
    [CreateAssetMenu(fileName = "SomeActionPerson", menuName = "SO/ActionPerson", order = 0)]
    
    
    
    public class SOActionPerson : SOPerson
    {
        public List<SkillTag> skillTags;
    }
}
