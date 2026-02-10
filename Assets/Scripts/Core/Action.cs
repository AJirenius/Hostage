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
    
    public class Action
    {
        public Verb verb;
        public SOActionPerson SoPerson;
        public float timeLeft;
        public float modifiedTime;



        public Action(Verb verb, SOActionPerson soPerson)
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