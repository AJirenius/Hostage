using Hostage.SO;

namespace Hostage.Core
{
    public struct IntelAddedSignal
    {
        public Intel Intel;
    }

    public struct IntelRemovedSignal
    {
        public Intel Intel;
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
