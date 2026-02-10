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

        public event Action<EventGraph, GraphContext> OnGraphRequested;

        private List<Action> _actions = new List<Action>();

        public ActionManager(PersonManager personManager)
        {
            _personManager = personManager;
        }
        
        public void HandleTime(float gameTime)
        {
            // loop through _actions in reverse order so you can remove if time is 0
            for (int i = _actions.Count - 1; i >= 0; i--)
            {
                Action action = _actions[i];
                action.timeLeft -= gameTime;
                Debug.Log("Action: " + action.timeLeft);
                if (action.timeLeft <= 0)
                {
                    // Execute event in Verb
                    ExecuteAction(i);
                    _actions.RemoveAt(i);
                }
            }
        }

        private void ExecuteAction(int index)
        {
            Action action = _actions[index];
            Debug.Log("ExecuteAction: " + action.verb.actionType);
            switch (action.verb.actionType)
            {
                case ActionType.Analyze:
                    
                    break;
                case ActionType.Interview:
                    break;
                case ActionType.Investigate:
                    Investigate v = (Investigate)action.verb;
                    if (v.result)
                    {
                        var person = _personManager.GetPerson(action.SoPerson);
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

        public void AddAction(Action action)
        {
            Debug.Log("Adding action" + action.verb.actionType);
            action.modifiedTime = action.verb.GetModifiedTime(action.SoPerson);
            action.timeLeft = action.modifiedTime;
            _actions.Add(action);
        }
    }
}