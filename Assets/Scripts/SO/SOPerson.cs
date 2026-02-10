using Hostage.Core;
using UnityEngine;

namespace Hostage.SO
{
    public abstract class SOPerson : ScriptableObject
    {
        public string Id;
        public string Name;
        public string FullName;
        public string Description;
        public Sprite Portrait;
        public PersonStatus defaultStatus;

    }
}