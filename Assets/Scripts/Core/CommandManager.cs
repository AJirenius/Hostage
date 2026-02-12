using System;
using System.Collections.Generic;
using Hostage.Graphs;
using Hostage.SO;
using UnityEngine;

namespace Hostage.Core
{
    public class CommandManager
    {
        private readonly SignalBus _signalBus;

        public event Action<EventGraph, GraphContext> OnGraphRequested;

        private List<TimedCommand> _commands = new List<TimedCommand>();

        public CommandManager(SignalBus signalBus)
        {
            _signalBus = signalBus;
        }
        
        public void HandleTime(float gameTime)
        {
            for (int i = _commands.Count - 1; i >= 0; i--)
            {
                TimedCommand timedCommand = _commands[i];
                timedCommand.timeLeft -= gameTime;
                _signalBus.Publish(new TimedCommandProgressSignal
                {
                    Person = timedCommand.Person,
                    PercentageLeft = timedCommand.GetPercentageLeft()
                });
                if (timedCommand.timeLeft <= 0)
                {
                    if (ExecuteTimedCommand(i))
                    {
                        _commands.RemoveAt(i);
                    }
                }
            }
        }

        private bool ExecuteTimedCommand(int index)
        {
            TimedCommand timedCommand = _commands[index];
            

            var context = new GraphContext(timedCommand.Person);
            context.IntVariables[GraphContext.TimedEventIndexKey] = timedCommand.timedEventIndex;
            
            if (timedCommand.verb != null)
            {
                Debug.Log("ExecuteCommmand: " + timedCommand.verb.CommandType + " index: " + timedCommand.timedEventIndex);
                if (timedCommand.verb.result != null)
                {
                    OnGraphRequested?.Invoke(timedCommand.verb.result, context);
                }
                else if (timedCommand.SoIntel?.masterGraph != null)
                {
                    context.IntVariables[GraphContext.ActionOutputKey] = timedCommand.verb.CommandType.ToOutputIndex();
                    OnGraphRequested?.Invoke(timedCommand.SoIntel.masterGraph, context);
                }
            }
            else
            {
                Debug.Log("ExecutePersonAction: " + timedCommand.Person.SOReference.Name + " index: " + timedCommand.timedEventIndex);
                context.Intel = timedCommand.SoIntel;
                OnGraphRequested?.Invoke(timedCommand.Person.SOReference.personMasterGraph, context);
            }

            

            if (timedCommand.timedEvents != null && timedCommand.timedEventIndex < timedCommand.timedEvents.Count)
            {
                timedCommand.timeLeft = timedCommand.timedEvents[timedCommand.timedEventIndex].time;
                timedCommand.modifiedTime = timedCommand.timeLeft;
                timedCommand.timedEventIndex++;
                return false;
            }

            timedCommand.Person.ClearOccupied();
            _signalBus.Publish(new TimedCommandCompletedSignal { TimedCommand = timedCommand });
            return true;
        }

        public void AddTimedCommand(TimedCommand timedCommand)
        {
            if (timedCommand.Person.IsOccupied() || timedCommand.Person.IsUnknown() || !timedCommand.Person.IsAvailable())
            {
                Debug.LogWarning($"Cannot add action: person '{timedCommand.Person.SOReference.Name}' is {timedCommand.Person.Flag}");
                return;
            }

            if (timedCommand.verb != null)
            {
                if (timedCommand.verb.requiredTags != null)
                {
                    foreach (var tag in timedCommand.verb.requiredTags)
                    {
                        if (!timedCommand.Person.SOReference.skillTags.Contains(tag))
                        {
                            Debug.LogWarning($"Cannot add action {timedCommand.verb.CommandType}: person '{timedCommand.Person.SOReference.Name}' missing required tag {tag}");
                            return;
                        }
                    }
                }

                Debug.Log("Adding action " + timedCommand.verb.CommandType);
                timedCommand.Person.SetOccupied();
                timedCommand.modifiedTime = timedCommand.verb.GetModifiedTime(timedCommand.Person.SOReference);
                timedCommand.timeLeft = timedCommand.modifiedTime;
                timedCommand.timedEvents = timedCommand.verb switch
                {
                    Surveillance s => s.timedEvents,
                    Analyze a => a.timedEvents,
                    _ => null
                };
            }
            else
            {
                Debug.Log("Adding verbless action for " + timedCommand.Person.SOReference.Name);
                timedCommand.Person.SetOccupied();
                timedCommand.modifiedTime = 0f;
                timedCommand.timeLeft = 0f;
            }

            _commands.Add(timedCommand);
            _signalBus.Publish(new TimedCommandStartedSignal { TimedCommand = timedCommand });
        }
        
    }
}