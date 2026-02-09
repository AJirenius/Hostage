using System.Collections.Generic;
using UnityEngine;

namespace SO
{
    public abstract class Person : ScriptableObject
    {
        public string Name;
        public string Description;
        public Sprite Portrait;
        
    }
    
    [CreateAssetMenu(fileName = "SomeActionPerson", menuName = "SO/ActionPerson", order = 0)]
    public class ActionPerson : Person
    {
        public List<SkillTag> skillTags;
    }
    
}