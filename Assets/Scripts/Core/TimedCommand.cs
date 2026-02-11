using Hostage.SO;

namespace Hostage.Core
{
    public enum ActionType
    {
        None,
        Investigate,
        Interview,
        Surveillance,
        Analyze,
    }

    public static class ActionTypeExtensions
    {
        public static int ToOutputIndex(this ActionType at) => (int)at - 1;
    }

    public class TimedCommand
    {
        public Verb verb;
        public Person Person;
        public Intel Intel;
        public float timeLeft;
        public float modifiedTime;

        public TimedCommand(Verb verb, Person person, Intel intel)
        {
            this.Person = person;
            this.verb = verb;
            this.Intel = intel;
        }

        public float GetPercentageLeft()
        {
            return timeLeft / modifiedTime;
        }

        public float GetTimeLeftSeconds()
        {
            // calculate to readable time in whole seconds
            return 0;
        }
    }
}