using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hostage.Core;
using Hostage.SO;

namespace Hostage.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Prefabs")] public GameObject intelCardPrefab;
        public GameObject personCardPrefab;
        public GameObject commandCardPrefab;
        [Header("Parents")] public Transform intelParent;
        public Transform personParent;
        public Transform commandCardParent;

        private PlayerInventory _playerInventory;
        private CommandManager _commandManager;
        private PersonManager _personManager;
        private SignalBus _signalBus;
        private Dictionary<SOIntel, GameObject > _createdIntelCards = new Dictionary<SOIntel, GameObject>();
        private Dictionary<Person, GameObject > _createdPersonCards = new Dictionary<Person, GameObject>();
        private CommandCardUI _activeCommandCard;
        private float _spawnX = -200;
        public void Initialize(PlayerInventory inventory, CommandManager commandManager, PersonManager personManager, SignalBus signalBus)
        {
            _playerInventory = inventory;
            _commandManager = commandManager;
            _personManager = personManager;
            _signalBus = signalBus;

            _signalBus.Subscribe<IntelAddedSignal>(OnIntelAdded);
            _signalBus.Subscribe<IntelRemovedSignal>(OnIntelRemoved);
            _signalBus.Subscribe<PersonFlagsChangedSignal>(OnPersonFlagsChanged);
            _signalBus.Subscribe<TimedCommandProgressSignal>(OnTimedCommandProgress);
            _signalBus.Subscribe<TimedCommandStartedSignal>(OnTimedCommandStarted);
            _signalBus.Subscribe<TimedCommandCompletedSignal>(OnTimedCommandCompleted);

            RefreshIntelCards();
            RefreshPersonCards();
        }

        private void OnDestroy()
        {
            if (_signalBus == null) return;
            _signalBus.Unsubscribe<IntelAddedSignal>(OnIntelAdded);
            _signalBus.Unsubscribe<IntelRemovedSignal>(OnIntelRemoved);
            _signalBus.Unsubscribe<PersonFlagsChangedSignal>(OnPersonFlagsChanged);
            _signalBus.Unsubscribe<TimedCommandProgressSignal>(OnTimedCommandProgress);
            _signalBus.Unsubscribe<TimedCommandStartedSignal>(OnTimedCommandStarted);
            _signalBus.Unsubscribe<TimedCommandCompletedSignal>(OnTimedCommandCompleted);
        }

        private void OnIntelAdded(IntelAddedSignal signal)
        {
            if (_createdIntelCards.ContainsKey(signal.SoIntel)) return;
            var card = Instantiate(intelCardPrefab, intelParent);
            var intelCard = card.GetComponent<IntelCardUI>();
            intelCard.Setup(signal.SoIntel, this);
            card.transform.localPosition += new Vector3(_spawnX, Random.Range(-50, 50), 0);
            _createdIntelCards[signal.SoIntel] = card;
            _spawnX += 200;
            if (_spawnX > 800) _spawnX = -200;
        }

        private void OnIntelRemoved(IntelRemovedSignal signal)
        {
            if (_createdIntelCards.TryGetValue(signal.SoIntel, out var cardGo))
            {
                Destroy(cardGo);
                _createdIntelCards.Remove(signal.SoIntel);
            }
        }

        private void RefreshIntelCards()
        {
            IReadOnlyList<SOIntel> inventoryIntels = _playerInventory.GetAllIntel();
            var keysToRemove = new List<SOIntel>();
            foreach (KeyValuePair<SOIntel, GameObject> kvp in _createdIntelCards)
            {
                if (!inventoryIntels.Contains(kvp.Key))
                {
                    Destroy(kvp.Value);
                    keysToRemove.Add(kvp.Key);
                }
            }
            foreach (var key in keysToRemove)
            {
                Debug.LogWarning("key:"+key+" was removed from inventory but still had a card. Removing card.");
                _createdIntelCards.Remove(key);
            }
            foreach (var intel in inventoryIntels)
            {
                
                if (!_createdIntelCards.ContainsKey(intel))
                {
                    var card = Instantiate(intelCardPrefab, intelParent);
                    var intelCard = card.GetComponent<IntelCardUI>();
                    intelCard.Setup(intel, this);
                    card.transform.localPosition += new Vector3(_spawnX, Random.Range(-50, 50), 0);
                    _createdIntelCards[intel] = card;
                    _spawnX += 200;
                    if (_spawnX > 800) _spawnX = -200; // reset position if too many cards
                }
            }
        }

        private void RefreshPersonCards()
        {
            // Remove cards for persons no longer available
            var toRemove = new List<Person>();
            foreach (var kvp in _createdPersonCards)
            {
                if (!kvp.Key.IsAvailable())
                {
                    Destroy(kvp.Value);
                    toRemove.Add(kvp.Key);
                }
            }
            foreach (var key in toRemove)
                _createdPersonCards.Remove(key);

            // Add cards for newly available persons
            var persons = _personManager.GetAllPersons();
            foreach (var person in persons)
            {
                if (!person.IsAvailable() || _createdPersonCards.ContainsKey(person))
                    continue;

                var card = Instantiate(personCardPrefab, personParent);
                var personCard = card.GetComponent<PersonCardUI>();
                personCard.Setup(person, this);
                _createdPersonCards[person] = card;
            }

            // Reposition all cards
            float x = -500;
            foreach (var kvp in _createdPersonCards)
            {
                kvp.Value.transform.localPosition = new Vector3(x, -200, 0);
                x += 250;
            }
        }

        private void OnTimedCommandProgress(TimedCommandProgressSignal signal)
        {
            if (_createdPersonCards.TryGetValue(signal.Person, out var cardGo))
            {
                var personCard = cardGo.GetComponent<PersonCardUI>();
                if (personCard != null)
                    personCard.UpdateProgress(signal.PercentageLeft);
            }
        }

        private void OnTimedCommandStarted(TimedCommandStartedSignal signal)
        {
            var verb = signal.PersonCommand.verb;
            if (verb != null && verb.occupyingIntel)
            {
                var soIntel = signal.PersonCommand.SoIntel;
                if (soIntel != null && _createdIntelCards.TryGetValue(soIntel, out var cardGo))
                {
                    var intelCard = cardGo.GetComponent<IntelCardUI>();
                    if (intelCard != null)
                        intelCard.SetLocked(true);
                }
            }
        }

        private void OnTimedCommandCompleted(TimedCommandCompletedSignal signal)
        {
            var verb = signal.PersonCommand.verb;
            if (verb != null && verb.occupyingIntel)
            {
                var soIntel = signal.PersonCommand.SoIntel;
                if (soIntel != null && _createdIntelCards.TryGetValue(soIntel, out var cardGo))
                {
                    var intelCard = cardGo.GetComponent<IntelCardUI>();
                    if (intelCard != null)
                        intelCard.SetLocked(false);
                }
            }
        }

        private void OnPersonFlagsChanged(PersonFlagsChangedSignal signal)
        {
            if (_createdPersonCards.TryGetValue(signal.Person, out var cardGo))
            {
                var personCard = cardGo.GetComponent<PersonCardUI>();
                if (personCard != null)
                    personCard.UpdatePersonFlag();
            }

            foreach (var kvp in _createdIntelCards)
            {
                if (kvp.Key.category == IntelCategory.Person && kvp.Key.person == signal.Person.SOReference)
                {
                    var intelCard = kvp.Value.GetComponent<IntelCardUI>();
                    if (intelCard != null)
                        intelCard.UpdateName(signal.Person);
                }
            }

            RefreshPersonCards();
        }

        public void OnIntelDragStarted(SOIntel soIntel)
        {
            foreach (var kvp in _createdPersonCards)
            {
                var personCard = kvp.Value.GetComponent<PersonCardUI>();
                if (personCard == null) continue;

                bool valid = !kvp.Key.IsOccupied() && kvp.Key.CanInteractWithIntel(soIntel);
                personCard.SetHighlight(valid);
            }

            if (_activeCommandCard != null && !_activeCommandCard.IsLocked && !_activeCommandCard.HasAttachedIntel)
            {
                if (_activeCommandCard.Person.CanInteractWithIntel(soIntel))
                    _activeCommandCard.PopulateSlots(soIntel);
            }
        }

        public void OnIntelDragEnded()
        {
            foreach (var kvp in _createdPersonCards)
            {
                var personCard = kvp.Value.GetComponent<PersonCardUI>();
                if (personCard != null)
                    personCard.SetHighlight(false);
            }

            if (_activeCommandCard != null && !_activeCommandCard.HasAttachedIntel)
                _activeCommandCard.ClearSlots();
        }

        public void ShowCommandCard(Person person)
        {
            if (_activeCommandCard != null && _activeCommandCard.Person == person)
            {
                DismissCommandCard();
                return;
            }

            DismissCommandCard();

            if (!person.IsOccupied())
                person.TryCreateCommand();

            var go = Instantiate(commandCardPrefab, commandCardParent);
            _activeCommandCard = go.GetComponent<CommandCardUI>();
            _activeCommandCard.Setup(person, this);
        }

        public void ShowCommandCardWithIntel(Person person, IntelCardUI intelCard)
        {
            ShowCommandCard(person);
            if (_activeCommandCard != null)
                _activeCommandCard.AutoAssignIntel(intelCard);
        }

        public void SubmitCommand(Person person)
        {
            if (person.Command == null) return;
            _commandManager.AddPersonCommand(person.Command);
            DismissCommandCard();
        }

        public void DismissCommandCard()
        {
            if (_activeCommandCard == null) return;
            _activeCommandCard.ClearPendingCommand();
            _activeCommandCard.Cleanup();
            Destroy(_activeCommandCard.gameObject);
            _activeCommandCard = null;
        }
    }
}
