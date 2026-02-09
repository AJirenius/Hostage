using System.Collections.Generic;
using UnityEngine;

namespace SO
{
    [CreateAssetMenu(fileName = "PersonList", menuName = "SO/PersonList", order = 0)]
    public class PersonList : ScriptableObject
    {
        List<Person> persons;
    }
}