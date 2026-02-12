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
        readonly SOPersonList _personList;
        readonly UIManager _uiManager;
        readonly SignalBus _signalBus;
        readonly GameClock _gameClock;

        public GameInitializer(ActionManager actionManager, PlayerInventory playerInventory, IntelProvider intelProvider, EventGraph eventGraph, EventGraphRunner eventGraphRunner,
            SOPersonList personList, UIManager uiManager, SignalBus signalBus, GameClock gameClock)
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
            _gameClock = gameClock;
        }

        public void Start()
        {
            _playerInventory.AddIntel(_intelProvider.GetWithId("IntelAlanBarker 3"));
            _eventGraphRunner.RunGraph(_eventGraph);
            _uiManager.Initialize(_playerInventory, _actionManager, _eventGraphRunner.PersonManager, _signalBus);
        }

        public void Tick()
        {
            _gameClock.Tick();
            _actionManager.HandleTime(_gameClock.GameDeltaTime);
        }
    }
}