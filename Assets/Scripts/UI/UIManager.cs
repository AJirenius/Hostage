using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hostage.Core;
using Hostage.SO;
using Random = UnityEngine.Random;

namespace Hostage.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Prefabs")] public GameObject intelCardPrefab;
        public GameObject personCardPrefab;
        public GameObject commandCardPrefab;
        public GameObject dialogueBoxPrefab;
        [Header("Parents")] public Transform intelParent;
        public Transform personParent;
        public Transform commandCardParent;
        public Transform dialogueBoxParent;

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
            _signalBus.Subscribe<PersonCommandUpdatedSignal>(OnPersonCommandUpdated);
            _signalBus.Subscribe<DialogueRequestedSignal>(OnDialogueRequested);

            RefreshIntelCards();
            RefreshPersonCards();
        }

        private void OnDestroy()
        {
            if (_signalBus == null) return;
            _signalBus.Unsubscribe<IntelAddedSignal>(OnIntelAdded);
            _signalBus.Unsubscribe<IntelRemovedSignal>(OnIntelRemoved);
            _signalBus.Unsubscribe<PersonFlagsChangedSignal>(OnPersonFlagsChanged);
            _signalBus.Unsubscribe<PersonCommandUpdatedSignal>(OnPersonCommandUpdated);
            _signalBus.Unsubscribe<DialogueRequestedSignal>(OnDialogueRequested);
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

        }

        private void OnPersonCommandUpdated(PersonCommandUpdatedSignal signal)
        {
            var personCommand = signal.PersonCommand;

            switch (signal.Status)
            {
                case PersonCommandStatus.Progress:
                    if (!personCommand.hideTime &&
                        _createdPersonCards.TryGetValue(personCommand.Person, out var progressCard))
                    {
                        var personCard = progressCard.GetComponent<PersonCardUI>();
                        if (personCard != null)
                            personCard.UpdateProgress(personCommand.GetPercentageLeft());
                    }
                    break;

                case PersonCommandStatus.Started:
                    if (personCommand.verb != null && personCommand.verb.occupyingIntel)
                    {
                        var soIntel = personCommand.SoIntel;
                        if (soIntel != null && _createdIntelCards.TryGetValue(soIntel, out var intelCardGo))
                        {
                            var intelCard = intelCardGo.GetComponent<IntelCardUI>();
                            if (intelCard != null)
                                intelCard.SetLocked(true);
                        }
                    }
                    if (personCommand.hideTime &&
                        _createdPersonCards.TryGetValue(personCommand.Person, out var startedCard))
                    {
                        var personCard = startedCard.GetComponent<PersonCardUI>();
                        if (personCard != null)
                            personCard.SetQuestionIcon(true);
                    }
                    break;

                case PersonCommandStatus.Ready:
                    if (_createdPersonCards.TryGetValue(personCommand.Person, out var readyCard))
                    {
                        var personCard = readyCard.GetComponent<PersonCardUI>();
                        if (personCard != null)
                        {
                            personCard.SetExclamationIcon(true);
                            personCard.UpdateProgress(0f);
                            personCard.SetQuestionIcon(false);
                        }
                    }
                    break;

                case PersonCommandStatus.Completed:
                case PersonCommandStatus.Cancelled:
                    if (personCommand.verb != null )
                    {
                        if (personCommand.verb.occupyingIntel)
                        {
                            var soIntel = personCommand.SoIntel;
                            if (soIntel != null && _createdIntelCards.TryGetValue(soIntel, out var intelCardGo))
                            {
                                var intelCard = intelCardGo.GetComponent<IntelCardUI>();
                                if (intelCard != null)
                                    intelCard.SetLocked(false);
                            }
                        }

                        if (_createdPersonCards.TryGetValue(personCommand.Person, out var cancelCard))
                        {
                            var personCard = cancelCard.GetComponent<PersonCardUI>();
                            if (personCard != null)
                            {
                                personCard.UpdateProgress(0f);
                                personCard.SetQuestionIcon(false);
                            }
                        }
                    }
                    break;
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

        private void OnDialogueRequested(DialogueRequestedSignal signal)
        {
            var go = Instantiate(dialogueBoxPrefab, dialogueBoxParent);
            var dialogueBox = go.GetComponent<DialogueBoxUI>();
            dialogueBox.Show(signal.SpeakerName, signal.Message, signal.OnDismissed);
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

        public void CancelCommand(Person person)
        {
            _commandManager.CancelCommand(person);
            DismissCommandCard();
        }

        public void ExecuteReadyCommand(Person person)
        {
            if (_createdPersonCards.TryGetValue(person, out var cardGo))
            {
                var personCard = cardGo.GetComponent<PersonCardUI>();
                if (personCard != null)
                    personCard.SetExclamationIcon(false);
            }
            _commandManager.ExecuteReadyCommand(person);
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
