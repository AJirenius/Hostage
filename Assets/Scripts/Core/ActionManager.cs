using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

namespace Core
{
    public class ActionManager
    {
        private List<Action> _actions = new List<Action>();
        
        public void HandleTime(float gameTime)
        {
            // loop through _actions in reverse order so you can remove if time is 0
            for (int i = _actions.Count - 1; i >= 0; i--)
            {
                Action action = _actions[i];
                action.timeLeft -= gameTime;
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
            switch (action.verb.actionType)
            {
                case ActionType.Analyze:
                    break;
                case ActionType.Interview:
                    break;
                case ActionType.Investigate:
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
            action.modifiedTime = action.verb.GetModifiedTime(action.person);
            action.timeLeft = action.modifiedTime;
            _actions.Add(action);
        }
        
    }

    
}