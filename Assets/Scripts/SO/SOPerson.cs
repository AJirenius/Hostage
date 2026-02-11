using Hostage.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace Hostage.SO
{
    public abstract class SOPerson : ScriptableObject
    {
        public string Id;
        public string Name;
        public string FullName;
        public string Description;
        public Sprite Portrait;
        [FormerlySerializedAs("defaultStatus")] public PersonFlag defaultFlag;

    }
}