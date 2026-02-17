using System;
using System.Collections.Generic;
using Hostage.Graphs;
using Hostage.SO;
using UnityEngine;

namespace Hostage.Core
{
    public class CommandManager
    {
        private const int MAX_ITERATIONS = 100; // Safety limit to prevent infinite loops

        private readonly SignalBus _signalBus;
        private readonly GameClock _gameClock;
        private readonly PlayerInventory _playerInventory;

        public event Action<EventGraph, GraphContext, Action<GraphResult>> OnGraphRequested;

        private List<PersonCommand> _commands = new List<PersonCommand>();
        private List<TimedEvents> _timedEvents = new List<TimedEvents>();

        public CommandManager(SignalBus signalBus, GameClock gameClock, PlayerInventory playerInventory)
        {
            _signalBus = signalBus;
            _gameClock = gameClock;
            _playerInventory = playerInventory;
        }

        public void HandleTime(float gameTime)
        {
            if (_gameClock.Paused) return;

            for (int i = _commands.Count - 1; i >= 0; i--)
            {
                PersonCommand personCommand = _commands[i];
                if (personCommand.readyToExecute) continue;

                personCommand.timeLeft -= gameTime;
                _signalBus.Publish(new PersonCommandUpdatedSignal
                {
                    PersonCommand = personCommand,
                    Status = PersonCommandStatus.Progress
                });
                if (personCommand.timeLeft <= 0)
                {
                    personCommand.readyToExecute = true;
                    _signalBus.Publish(new PersonCommandUpdatedSignal
                    {
                        PersonCommand = personCommand,
                        Status = PersonCommandStatus.Ready
                    });
                }
            }

            for (int i = _timedEvents.Count - 1; i >= 0; i--)
            {
                TimedEvents timedEvents = _timedEvents[i];
                timedEvents.timeLeft -= gameTime;
                if (timedEvents.timeLeft <= 0)
                {
                    ExecuteTimedEvents(timedEvents);
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
                Debug.Log("Execute Assistant commmand: " + personCommand.Person.SOReference.Name + " Verb:" + personCommand.verb.CommandType + " index: " + personCommand.timedEventIndex);
                if (personCommand.SoIntel?.graph != null)
                {
                    context.IntVariables[GraphContext.VerbTypeIndexKey] = personCommand.verb.CommandType.ToOutputIndex();
                    graph = personCommand.SoIntel.graph;
                }
            }
            else
            {
                Debug.Log("Execute NPC Command: " + personCommand.Person.SOReference.Name + " index: " + personCommand.timedEventIndex);
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

            // Check if graph wants to schedule another iteration
            if (result.ScheduleNextIteration && personCommand.timedEventIndex < MAX_ITERATIONS)
            {
                personCommand.readyToExecute = false;
                personCommand.timeLeft = result.NextIterationTime;
                personCommand.modifiedTime = personCommand.timeLeft;
                personCommand.hideTime = result.HideTime;
                personCommand.timedEventIndex++;
                _signalBus.Publish(new PersonCommandUpdatedSignal
                {
                    PersonCommand = personCommand,
                    Status = PersonCommandStatus.Started
                });
                return;
            }

            // Finishing up
            if (!result.AllowRepeat)
            {
                var commandType = personCommand.verb?.CommandType ?? CommandType.None;
                personCommand.Person.RecordCompletedCommand(personCommand.SoIntel, commandType);
            }

            // Return intel to player inventory if applicable
            if (personCommand.verb != null &&
                personCommand.verb.occupyingIntel &&
                personCommand.SoIntel != null &&
                result.ReturnIntel)
            {
                if (personCommand.Person.RemoveIntel(personCommand.SoIntel))
                {
                    _playerInventory.AddIntel(personCommand.SoIntel);
                }
            }

            personCommand.Person.ClearOccupied();
            personCommand.Person.ClearCommand();
            _commands.Remove(personCommand);
            _signalBus.Publish(new PersonCommandUpdatedSignal
            {
                PersonCommand = personCommand,
                Status = PersonCommandStatus.Completed
            });
        }

        private void ExecuteTimedEvents(TimedEvents timedEvents)
        {
            _gameClock.Paused = true;

            var context = new GraphContext();
            context.IntVariables[GraphContext.TimedEventIndexKey] = timedEvents.timedEventIndex;

            EventGraph graph = timedEvents.SOTimedEvents.eventGraph;

            if (graph != null && OnGraphRequested != null)
            {
                OnGraphRequested.Invoke(graph, context, result => OnTimedEventsGraphCompleted(timedEvents, result));
            }
            else
            {
                OnTimedEventsGraphCompleted(timedEvents, new GraphResult());
            }
        }

        private void OnTimedEventsGraphCompleted(TimedEvents timedEvents, GraphResult result)
        {
            _gameClock.Paused = false;

            timedEvents.timedEventIndex++;

            // Check if graph wants to schedule another iteration
            if (result.ScheduleNextIteration && timedEvents.timedEventIndex < MAX_ITERATIONS)
            {
                timedEvents.timeLeft = result.NextIterationTime;
                timedEvents.currentFullTime = timedEvents.timeLeft;
                return;
            }

            _timedEvents.Remove(timedEvents);
        }

        public void AddTimedEvents(SOTimedEvents soTimedEvents)
        {
            var timedEvents = new TimedEvents(soTimedEvents);
            timedEvents.timeLeft = soTimedEvents.initialTime;
            timedEvents.currentFullTime = timedEvents.timeLeft;
            timedEvents.timedEventIndex = 0;
            _timedEvents.Add(timedEvents);
        }

        public void ExecuteReadyCommand(Person person)
        {
            for (int i = _commands.Count - 1; i >= 0; i--)
            {
                if (_commands[i].Person == person && _commands[i].readyToExecute)
                {
                    ExecuteTimedCommand(_commands[i]);
                    return;
                }
            }
        }

        public void CancelCommand(Person person)
        {
            for (int i = _commands.Count - 1; i >= 0; i--)
            {
                var cmd = _commands[i];
                if (cmd.Person != person || cmd.readyToExecute) continue;

                // Return intel to player inventory if applicable
                if (cmd.verb != null && cmd.verb.occupyingIntel && cmd.SoIntel != null)
                {
                    if (person.RemoveIntel(cmd.SoIntel))
                        _playerInventory.AddIntel(cmd.SoIntel);
                }

                person.ClearOccupied();
                person.ClearCommand();
                _commands.RemoveAt(i);
                _signalBus.Publish(new PersonCommandUpdatedSignal
                {
                    PersonCommand = cmd,
                    Status = PersonCommandStatus.Cancelled
                });
                return;
            }
        }

        public void AddPersonCommand(PersonCommand personCommand)
        {
            if (personCommand.verb != null)
            {
                personCommand.Person.SetOccupied();
                personCommand.modifiedTime = personCommand.verb.GetModifiedTime(personCommand.Person.SOReference);
                personCommand.timeLeft = personCommand.modifiedTime;
                personCommand.hideTime = personCommand.verb.hideTime;

                // Transfer intel to person if occupyingIntel is true
                if (personCommand.verb.occupyingIntel && personCommand.SoIntel != null)
                {
                    if (_playerInventory.RemoveIntel(personCommand.SoIntel))
                    {
                        personCommand.Person.AddIntel(personCommand.SoIntel);
                    }
                }
            }
            else
            {
                personCommand.Person.SetOccupied();
                personCommand.modifiedTime = 0f;
                personCommand.timeLeft = 0f;
            }

            _commands.Add(personCommand);
            _signalBus.Publish(new PersonCommandUpdatedSignal
            {
                PersonCommand = personCommand,
                Status = PersonCommandStatus.Started
            });
        }

    }
}
