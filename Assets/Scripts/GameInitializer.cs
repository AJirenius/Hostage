using Hostage.Core;
using Hostage.SO;
using UnityEngine;
using VContainer.Unity;

namespace Hostage
{
    public class GameInitializer: ITickable, IStartable
    {
        readonly ActionManager _actionManager;
        readonly PlayerInventory _playerInventory;
        readonly IntelProvider _intelProvider;

        public GameInitializer(ActionManager actionManager, PlayerInventory playerInventory, IntelProvider intelProvider)
        {
            Debug.Log("Initializing player inventory");
            _actionManager = actionManager;
            _playerInventory = playerInventory;
            _intelProvider = intelProvider;
        }
        
        public void Start()
        {
            _playerInventory.AddIntel(_intelProvider.GetWithId("IntelAlanBarker 3"));
        }

        public void Tick()
        {   
            float deltaTime = UnityEngine.Time.deltaTime; // fix later on with modifiable gametime
            _actionManager.HandleTime(deltaTime);
        }

        
    }
}