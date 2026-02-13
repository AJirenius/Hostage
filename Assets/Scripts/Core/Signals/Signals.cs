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

    public struct TimedCommandProgressSignal
    {
        public Person Person;
        public float PercentageLeft;
    }

    public struct PersonFlagsChangedSignal
    {
        public Person Person;
    }

    public struct PersonListChangedSignal { }

    public struct FlagChangedSignal
    {
        public Flag Flag;
        public bool IsSet;
    }
}
