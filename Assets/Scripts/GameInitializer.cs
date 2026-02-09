using Core;
using UnityEngine;
using VContainer.Unity;

namespace DefaultNamespace
{
    public class GameInitializer: MonoBehaviour
    {
        readonly ActionManager _actionManager;
        readonly PlayerInventory _playerInventory;

        public GameInitializer(ActionManager actionManager, PlayerInventory playerInventory)
        {
            _actionManager = actionManager;
            _playerInventory = playerInventory;
            
        }

        public void Tick()
        {   
            float deltaTime = UnityEngine.Time.deltaTime; // fix later on with modifiable gametime
            _actionManager.HandleTime(deltaTime);
        }
    }
}