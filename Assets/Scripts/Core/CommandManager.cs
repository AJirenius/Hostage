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

        private List<PersonCommand> _commands = new List<PersonCommand>();

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
                PersonCommand personCommand = _commands[i];
                personCommand.timeLeft -= gameTime;
                _signalBus.Publish(new TimedCommandProgressSignal
                {
                    Person = personCommand.Person,
                    PercentageLeft = personCommand.GetPercentageLeft()
                });
                if (personCommand.timeLeft <= 0)
                {
                    ExecuteTimedCommand(personCommand);
                    if (_gameClock.Paused) return;
                }
            }
        }

        private void ExecuteTimedCommand(PersonCommand personCommand)
        {
            _gameClock.Paused = true;

            var context = new GraphContext(personCommand.Person);
            context.IntVariables[GraphContext.TimedEventIndexKey] = personCommand.timedEventIndex;

            EventGraph graph = null;

            if (personCommand.verb != null)
            {
                Debug.Log("ExecuteCommmand: " + personCommand.verb.CommandType + " index: " + personCommand.timedEventIndex);
                if (personCommand.SoIntel?.graph != null)
                {
                    context.IntVariables[GraphContext.ActionOutputKey] = personCommand.verb.CommandType.ToOutputIndex();
                    graph = personCommand.SoIntel.graph;
                }
            }
            else
            {
                Debug.Log("ExecutePersonAction: " + personCommand.Person.SOReference.Name + " index: " + personCommand.timedEventIndex);
                context.Intel = personCommand.SoIntel;
                graph = personCommand.Person.SOReference.personMasterGraph;
            }

            if (graph != null && OnGraphRequested != null)
            {
                OnGraphRequested.Invoke(graph, context, result => OnGraphCompleted(personCommand, result));
            }
            else
            {
                OnGraphCompleted(personCommand, new GraphResult());
            }
        }

        private void OnGraphCompleted(PersonCommand personCommand, GraphResult result)
        {
            _gameClock.Paused = false;

            if (personCommand.timedEvents != null && personCommand.timedEventIndex < personCommand.timedEvents.Count)
            {
                personCommand.timeLeft = personCommand.timedEvents[personCommand.timedEventIndex].time;
                personCommand.modifiedTime = personCommand.timeLeft;
                personCommand.timedEventIndex++;
                return;
            }

            // Finishing up
            if (!result.AllowRepeat)
            {
                var commandType = personCommand.verb?.CommandType ?? CommandType.None;
                personCommand.Person.RecordCompletedCommand(personCommand.SoIntel, commandType);
            }

            personCommand.Person.ClearOccupied();
            personCommand.Person.ClearCommand();
            _commands.Remove(personCommand);
            _signalBus.Publish(new TimedCommandCompletedSignal { PersonCommand = personCommand });
        }

        public void AddTimedCommand(PersonCommand personCommand)
        {
            if (personCommand.verb != null)
            {
                personCommand.Person.SetOccupied();
                personCommand.modifiedTime = personCommand.verb.GetModifiedTime(personCommand.Person.SOReference);
                personCommand.timeLeft = personCommand.modifiedTime;
                personCommand.timedEvents = personCommand.verb switch
                {
                    Surveillance s => s.timedEvents,
                    Analyze a => a.timedEvents,
                    _ => null
                };
            }
            else
            {
                personCommand.Person.SetOccupied();
                personCommand.modifiedTime = 0f;
                personCommand.timeLeft = 0f;
            }

            _commands.Add(personCommand);
            _signalBus.Publish(new TimedCommandStartedSignal { PersonCommand = personCommand });
        }

    }
}
