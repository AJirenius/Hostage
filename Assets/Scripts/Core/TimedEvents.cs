using Hostage.SO;

namespace Hostage.Core
{
    public class TimedEvents
    {
        public SOTimedEvents SOTimedEvents;
        public float timeLeft;
        public float currentFullTime;
        public int timedEventIndex;

        public TimedEvents(SOTimedEvents SOTimedEvents)
        {
            this.SOTimedEvents = SOTimedEvents;
        }

        public float GetPercentageLeft()
        {
            if (currentFullTime == 0) return 0;
            return timeLeft / currentFullTime;
        }
    }
}