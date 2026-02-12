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
        [Header("Parents")] public Transform intelParent;
        public Transform personParent;

        private PlayerInventory _playerInventory;
        private ActionManager _actionManager;
        private PersonManager _personManager;
        private SignalBus _signalBus;
        private Dictionary<SOIntel, GameObject > _createdIntelCards = new Dictionary<SOIntel, GameObject>();
        private Dictionary<Person, GameObject > _createdPersonCards = new Dictionary<Person, GameObject>();
        private float _spawnX = -200;
        public void Initialize(PlayerInventory inventory, ActionManager actionManager, PersonManager personManager, SignalBus signalBus)
        {
            _playerInventory = inventory;
            _actionManager = actionManager;
            _personManager = personManager;
            _signalBus = signalBus;

            _signalBus.Subscribe<IntelAddedSignal>(OnIntelAdded);
            _signalBus.Subscribe<IntelRemovedSignal>(OnIntelRemoved);
            _signalBus.Subscribe<PersonStatusChangedSignal>(OnPersonStatusChanged);
            _signalBus.Subscribe<TimedCommandProgressSignal>(OnTimedCommandProgress);

            RefreshIntelCards();
            RefreshPersonCards();
        }

        private void OnDestroy()
        {
            if (_signalBus == null) return;
            _signalBus.Unsubscribe<IntelAddedSignal>(OnIntelAdded);
            _signalBus.Unsubscribe<IntelRemovedSignal>(OnIntelRemoved);
            _signalBus.Unsubscribe<PersonStatusChangedSignal>(OnPersonStatusChanged);
            _signalBus.Unsubscribe<TimedCommandProgressSignal>(OnTimedCommandProgress);
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

        private void OnPersonStatusChanged(PersonStatusChangedSignal signal)
        {
            if (_createdPersonCards.TryGetValue(signal.Person, out var cardGo))
            {
                var personCard = cardGo.GetComponent<PersonCardUI>();
                if (personCard != null)
                    personCard.UpdateStatus();
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

        public void OnVerbSelected(Verb verb, Person person, SOIntel soIntel)
        {
            var command = new TimedCommand(verb, person, soIntel);
            _actionManager.AddTimedCommand(command);
            ClearAllCommandButtons();
        }

        public void OnButtonSelected(Person person, SOIntel soIntel)
        {
            var command = new TimedCommand(null, person, soIntel);
            _actionManager.AddTimedCommand(command);
            ClearAllCommandButtons();
        }

        public void ClearAllCommandButtons()
        {
            foreach (var kvp in _createdPersonCards)
            {
                var personCard = kvp.Value.GetComponent<PersonCardUI>();
                if (personCard != null)
                    personCard.ClearCommandButtons();
            }
        }
    }
}
