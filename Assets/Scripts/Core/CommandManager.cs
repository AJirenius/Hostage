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
        private readonly GameClock _gameClock;

        public event Action<EventGraph, GraphContext, Action<GraphResult>> OnGraphRequested;

        private List<TimedCommand> _commands = new List<TimedCommand>();

        public CommandManager(SignalBus signalBus, GameClock gameClock)
        {
            _signalBus = signalBus;
            _gameClock = gameClock;
        }

        public void HandleTime(float gameTime)
        {
            if (_gameClock.Paused) return;

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
                    ExecuteTimedCommand(timedCommand);
                    if (_gameClock.Paused) return;
                }
            }
        }

        private void ExecuteTimedCommand(TimedCommand timedCommand)
        {
            _gameClock.Paused = true;

            var context = new GraphContext(timedCommand.Person);
            context.IntVariables[GraphContext.TimedEventIndexKey] = timedCommand.timedEventIndex;

            EventGraph graph = null;

            if (timedCommand.verb != null)
            {
                Debug.Log("ExecuteCommmand: " + timedCommand.verb.CommandType + " index: " + timedCommand.timedEventIndex);
                if (timedCommand.verb.result != null)
                {
                    graph = timedCommand.verb.result;
                }
                else if (timedCommand.SoIntel?.masterGraph != null)
                {
                    context.IntVariables[GraphContext.ActionOutputKey] = timedCommand.verb.CommandType.ToOutputIndex();
                    graph = timedCommand.SoIntel.masterGraph;
                }
            }
            else
            {
                Debug.Log("ExecutePersonAction: " + timedCommand.Person.SOReference.Name + " index: " + timedCommand.timedEventIndex);
                context.Intel = timedCommand.SoIntel;
                graph = timedCommand.Person.SOReference.personMasterGraph;
            }

            if (graph != null && OnGraphRequested != null)
            {
                OnGraphRequested.Invoke(graph, context, result => OnGraphCompleted(timedCommand, result));
            }
            else
            {
                OnGraphCompleted(timedCommand, new GraphResult());
            }
        }

        private void OnGraphCompleted(TimedCommand timedCommand, GraphResult result)
        {
            _gameClock.Paused = false;

            if (timedCommand.timedEvents != null && timedCommand.timedEventIndex < timedCommand.timedEvents.Count)
            {
                timedCommand.timeLeft = timedCommand.timedEvents[timedCommand.timedEventIndex].time;
                timedCommand.modifiedTime = timedCommand.timeLeft;
                timedCommand.timedEventIndex++;
                return;
            }

            // Finishing up
            if (!result.AllowRepeat)
            {
                var commandType = timedCommand.verb?.CommandType ?? CommandType.None;
                timedCommand.Person.RecordCompletedCommand(timedCommand.SoIntel, commandType);
            }

            timedCommand.Person.ClearOccupied();
            timedCommand.Person.ClearCommand();
            _commands.Remove(timedCommand);
            _signalBus.Publish(new TimedCommandCompletedSignal { TimedCommand = timedCommand });
        }

        public void AddTimedCommand(TimedCommand timedCommand)
        {
            if (timedCommand.verb != null)
            {
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
                timedCommand.Person.SetOccupied();
                timedCommand.modifiedTime = 0f;
                timedCommand.timeLeft = 0f;
            }

            _commands.Add(timedCommand);
            _signalBus.Publish(new TimedCommandStartedSignal { TimedCommand = timedCommand });
        }

    }
}
