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
        readonly CommandManager _commandManager;
        readonly PlayerInventory _playerInventory;
        readonly IntelProvider _intelProvider;
        readonly SOTimedEvents _soTimedEvents;
        readonly EventGraphRunner _eventGraphRunner;
        readonly SOPersonList _personList;
        readonly UIManager _uiManager;
        readonly SignalBus _signalBus;
        readonly GameClock _gameClock;

        public GameInitializer(CommandManager commandManager, PlayerInventory playerInventory, IntelProvider intelProvider, SOTimedEvents timedEvents, EventGraphRunner eventGraphRunner,
            SOPersonList personList, UIManager uiManager, SignalBus signalBus, GameClock gameClock)
        {
            Debug.Log("Initializing player inventory");
            _commandManager = commandManager;
            _playerInventory = playerInventory;
            _intelProvider = intelProvider;
            _soTimedEvents = timedEvents;
            _eventGraphRunner = eventGraphRunner;
            _personList = personList;
            _uiManager = uiManager;
            _signalBus = signalBus;
            _gameClock = gameClock;
        }

        public void Start()
        {
            _playerInventory.AddIntel(_intelProvider.GetWithId("IntelAlanBarker 3"));
            _uiManager.Initialize(_playerInventory, _commandManager, _eventGraphRunner.PersonManager, _signalBus);
            _commandManager.AddTimedEvents(_soTimedEvents);
        }

        public void Tick()
        {
            _gameClock.Tick();
            _commandManager.HandleTime(_gameClock.GameDeltaTime);
        }
    }
}