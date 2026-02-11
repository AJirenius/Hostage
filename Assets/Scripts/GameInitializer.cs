using Hostage.Core;
using Hostage.Graphs;
using Hostage.SO;
using Hostage.UI;
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
        readonly SOActionPersonList _personList;
        readonly UIManager _uiManager;
        readonly SignalBus _signalBus;

        public GameInitializer(ActionManager actionManager, PlayerInventory playerInventory, IntelProvider intelProvider, EventGraph eventGraph, EventGraphRunner eventGraphRunner,
            SOActionPersonList personList, UIManager uiManager, SignalBus signalBus)
        {
            Debug.Log("Initializing player inventory");
            _actionManager = actionManager;
            _playerInventory = playerInventory;
            _intelProvider = intelProvider;
            _eventGraph = eventGraph;
            _eventGraphRunner = eventGraphRunner;
            _personList = personList;
            _uiManager = uiManager;
            _signalBus = signalBus;
        }

        public void Start()
        {
            _playerInventory.AddIntel(_intelProvider.GetWithId("IntelAlanBarker 3"));
            _eventGraphRunner.RunGraph(_eventGraph);
            _uiManager.Initialize(_playerInventory, _actionManager, _eventGraphRunner.PersonManager, _signalBus);
        }

        public void Tick()
        {   
            float deltaTime = UnityEngine.Time.deltaTime; // fix later on with modifiable gametime
            _actionManager.HandleTime(deltaTime);
        }
    }
}