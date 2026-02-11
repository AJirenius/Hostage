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

    public struct ActionAddedSignal
    {
        public TimedCommand TimedCommand;
    }

    public struct ActionCompletedSignal
    {
        public TimedCommand TimedCommand;
    }

    public struct PersonStatusChangedSignal
    {
        public Person Person;
    }

    public struct PersonListChangedSignal { }
}
