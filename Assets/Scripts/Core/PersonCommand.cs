using Hostage.SO;

namespace Hostage.Core
{
    public enum CommandType
    {
        None,
        Investigate,
        Interview,
        Surveillance,
        Analyze,
    }

    public static class ActionTypeExtensions
    {
        public static int ToOutputIndex(this CommandType at) => (int)at - 1;
    }

    public class PersonCommand
    {
        public Verb verb;
        public Person Person;
        public SOIntel SoIntel;
        public float timeLeft;
        public float modifiedTime;
        public int timedEventIndex;

        public PersonCommand(Person person)
        {
            this.Person = person;
        }

        public float GetPercentageLeft()
        {
            if (modifiedTime == 0) return 0;
            return timeLeft / modifiedTime;
        }

        public float GetTimeLeftSeconds()
        {
            // calculate to readable time in whole seconds
            return 0;
        }
    }
}