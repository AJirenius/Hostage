using System;
using System.Collections.Generic;
using Hostage.Graphs;
using Hostage.SO;
using UnityEngine;

namespace Hostage.Core
{
    public class ActionManager
    {
        private readonly PersonManager _personManager;
        private readonly SignalBus _signalBus;

        public event Action<EventGraph, GraphContext> OnGraphRequested;

        private List<TimedCommand> _commands = new List<TimedCommand>();

        public ActionManager(PersonManager personManager, SignalBus signalBus)
        {
            _personManager = personManager;
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
            _signalBus.Publish(new ActionCompletedSignal { TimedCommand = timedCommand });
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
                        var person = _personManager.GetPerson(timedCommand.SoPerson);
                        var context = new GraphContext(person);
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

        public void AddAction(TimedCommand timedCommand)
        {
            Debug.Log("Adding action" + timedCommand.verb.actionType);
            timedCommand.modifiedTime = timedCommand.verb.GetModifiedTime(timedCommand.SoPerson);
            timedCommand.timeLeft = timedCommand.modifiedTime;
            _commands.Add(timedCommand);
            _signalBus.Publish(new ActionAddedSignal { TimedCommand = timedCommand });
        }
    }
}