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
    
    public class TimedCommand
    {
        public Verb verb;
        public SOActionPerson SoPerson;
        public float timeLeft;
        public float modifiedTime;



        public TimedCommand(Verb verb, SOActionPerson soPerson)
        {
            this.SoPerson = soPerson;
            this.verb = verb;
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