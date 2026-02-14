using System;
using Hostage.Core;
using UnityEngine;

namespace Hostage.SO
{
    [CreateAssetMenu(fileName = "PersonDefaultMessages", menuName = "SO/PersonMessages", order = 0)]
    public class SOPersonDefaultMessages : ScriptableObject
    {
        public PersonDefaultCategoryMessages categoryMessages;

        public string GetMessage(IntelCategory category, CommandType commandType)
        {
            var verbMessages = category switch
            {
                IntelCategory.PhoneNumber => categoryMessages.phoneNumber,
                IntelCategory.Person => categoryMessages.person,
                IntelCategory.Location => categoryMessages.location,
                IntelCategory.Document => categoryMessages.document,
                _ => categoryMessages.unknown,
            };

            return commandType switch
            {
                CommandType.Investigate => verbMessages.investigate,
                CommandType.Interview => verbMessages.interview,
                CommandType.Surveillance => verbMessages.surveillance,
                CommandType.Analyze => verbMessages.analyze,
                _ => "",
            };
        }
    }

    [Serializable]
    public struct PersonDefaultCategoryMessages
    {
        public PersonDefaultVerbMessages unknown;
        public PersonDefaultVerbMessages phoneNumber;
        public PersonDefaultVerbMessages person;
        public PersonDefaultVerbMessages location;
        public PersonDefaultVerbMessages document;
    }

    [Serializable]
    public struct PersonDefaultVerbMessages
    {
        public string investigate;
        public string interview;
        public string surveillance;
        public string analyze;
    }
}
