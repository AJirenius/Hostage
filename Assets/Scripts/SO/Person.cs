using UnityEngine;

namespace Hostage.SO
{
    public abstract class Person : ScriptableObject
    {
        public string Name;
        public string Description;
        public Sprite Portrait;
    }
}