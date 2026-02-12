using Hostage.SO;

namespace Hostage.Core
{
    public struct IntelAddedSignal
    {
        public SOIntel SoIntel;
    }

    public struct IntelRemovedSignal
    {
        public SOIntel SoIntel;
    }

    public struct TimedCommandStartedSignal
    {
        public TimedCommand TimedCommand;
    }

    public struct TimedCommandCompletedSignal
    {
        public TimedCommand TimedCommand;
    }

    public struct PersonStatusChangedSignal
    {
        public Person Person;
    }

    public struct PersonListChangedSignal { }
}
