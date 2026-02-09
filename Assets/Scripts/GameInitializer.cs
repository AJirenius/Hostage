using Hostage.Core;
using Hostage.Graphs;
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
        readonly EventGraph _eventGraph;
        readonly EventGraphRunner _eventGraphRunner;

        public GameInitializer(ActionManager actionManager, PlayerInventory playerInventory, IntelProvider intelProvider, EventGraph eventGraph, EventGraphRunner eventGraphRunner)
        {
            Debug.Log("Initializing player inventory");
            _actionManager = actionManager;
            _playerInventory = playerInventory;
            _intelProvider = intelProvider;
            _eventGraph = eventGraph;
            _eventGraphRunner = eventGraphRunner;
        }
        
        public void Start()
        {
            _playerInventory.AddIntel(_intelProvider.GetWithId("IntelAlanBarker 3"));
            _eventGraphRunner.RunGraph(_eventGraph);
        }

        public void Tick()
        {   
            float deltaTime = UnityEngine.Time.deltaTime; // fix later on with modifiable gametime
            _actionManager.HandleTime(deltaTime);
        }

        
    }
}