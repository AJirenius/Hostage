using System;
using System.Collections.Generic;

namespace Hostage.Core
{
    public enum FlagScope
    {
        Chapter,
        Game,
        Experience
    }

    public enum Flag
    {
        // Chapter scope (0–99) — cleared each chapter
        ChapterIntroSeen = 0,
        EvidenceFound,
        WitnessInterviewed,

        // Game scope (100–199) — cleared each session
        TutorialSeen = 100,
        IntroComplete,
        FirstInterrogation,

        // Experience scope (200+) — persistent across sessions
        FirstTimePlaying = 200,
        OnboardingComplete,
        EvidenceReviewed,
    }

    public class FlagManager
    {
        readonly SignalBus _signalBus;
        readonly Dictionary<FlagScope, HashSet<Flag>> _flags = new();

        public FlagManager(SignalBus signalBus)
        {
            _signalBus = signalBus;
            foreach (FlagScope scope in Enum.GetValues(typeof(FlagScope)))
                _flags[scope] = new HashSet<Flag>();
        }

        public void SetFlag(Flag flag)
        {
            if (_flags[ScopeOf(flag)].Add(flag))
                _signalBus.Publish(new FlagChangedSignal { Flag = flag, IsSet = true });
        }

        public void ClearFlag(Flag flag)
        {
            if (_flags[ScopeOf(flag)].Remove(flag))
                _signalBus.Publish(new FlagChangedSignal { Flag = flag, IsSet = false });
        }

        public bool HasFlag(Flag flag) => _flags[ScopeOf(flag)].Contains(flag);

        public void ClearScope(FlagScope scope) => _flags[scope].Clear();

        public static FlagScope ScopeOf(Flag flag) => (int)flag switch
        {
            < 100 => FlagScope.Chapter,
            < 200 => FlagScope.Game,
            _ => FlagScope.Experience,
        };
    }
}
