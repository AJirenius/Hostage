using System;
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

    public enum PersonCommandStatus
    {
        Started,
        Progress,
        Ready,
        Completed,
        Cancelled
    }

    public struct PersonCommandUpdatedSignal
    {
        public PersonCommand PersonCommand;
        public PersonCommandStatus Status;
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

    public struct DialogueRequestedSignal
    {
        public string SpeakerName;
        public string Message;
        public Action OnDismissed;
    }

    public struct GraphStartedSignal { }
    public struct GraphCompletedSignal { }

    public struct DialogueChoiceRequestedSignal
    {
        public string SpeakerName;
        public string Message;
        public System.Collections.Generic.List<string> Options;
        public Action<int> OnOptionSelected;
    }
}
