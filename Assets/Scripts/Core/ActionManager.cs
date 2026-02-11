using System;
using System.Collections.Generic;
using Hostage.Graphs;
using Hostage.SO;
using UnityEngine;

namespace Hostage.Core
{
    public class ActionManager
    {
        private readonly SignalBus _signalBus;

        public event Action<EventGraph, GraphContext> OnGraphRequested;

        private List<TimedCommand> _commands = new List<TimedCommand>();

        public ActionManager(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }
        
        public void HandleTime(float gameTime)
        {
            // loop through _actions in reverse order so you can remove if time is 0
            for (int i = _commands.Count - 1; i >= 0; i--)
            {
                TimedCommand timedCommand = _commands[i];
                timedCommand.timeLeft -= gameTime;
                Debug.Log("Action: " + timedCommand.timeLeft);
                if (timedCommand.timeLeft <= 0)
                {
                    // Execute event in Verb
                    ExecuteAction(i);
                    _commands.RemoveAt(i);
                }
            }
        }

        private void ExecuteAction(int index)
        {
            TimedCommand timedCommand = _commands[index];
            timedCommand.Person.ClearOccupied();
            _signalBus.Publish(new TimedCommandCompletedSignal { TimedCommand = timedCommand });
            Debug.Log("ExecuteAction: " + timedCommand.verb.actionType);
            switch (timedCommand.verb.actionType)
            {
                case ActionType.Analyze:
                    
                    break;
                case ActionType.Interview:
                    break;
                case ActionType.Investigate:
                    Investigate v = (Investigate)timedCommand.verb;
                    if (v.result)
                    {
                        var context = new GraphContext(timedCommand.Person);
                        OnGraphRequested?.Invoke(v.result, context);
                    }
                    break;
                case ActionType.Surveillance:
                    // loop through timestamps and execeute whatever. Dont remove action from list
                    break;
                default:
                    break;
            }
        }

        public void AddTimedCommand(TimedCommand timedCommand)
        {
            if (timedCommand.Person.IsOccupied() || timedCommand.Person.IsUnknown() || !timedCommand.Person.IsAvailable())
            {
                Debug.LogWarning($"Cannot add action {timedCommand.verb.actionType}: person '{timedCommand.Person.SOReference.Name}' is {timedCommand.Person.Flag}");
                return;
            }

            Debug.Log("Adding action" + timedCommand.verb.actionType);
            timedCommand.Person.SetOccupied();
            timedCommand.modifiedTime = timedCommand.verb.GetModifiedTime(timedCommand.Person.SOReference);
            timedCommand.timeLeft = timedCommand.modifiedTime;
            _commands.Add(timedCommand);
            _signalBus.Publish(new TimedCommandStartedSignal { TimedCommand = timedCommand });
        }
    }
}