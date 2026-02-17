using System.Collections.Generic;
using Hostage.Core;
using Hostage.SO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Hostage.UI
{
    public class CommandCardUI : MonoBehaviour, IDropHandler
    {
        public RectTransform slotGroup;
        public TMPro.TMP_Text personNameText;
        public TMPro.TMP_Text textArea;
        public Image profileIcon;
        public GameObject intelSlotUIPrefab;
        public Button actionButton;
        public TMPro.TMP_Text actionButtonText;
        public Button closeButton;

        private Person _person;
        private UIManager _uiManager;
        private IntelCardUI _attachedIntelCard;
        private bool _isLocked;

        public Person Person => _person;
        public bool IsLocked => _isLocked;
        public bool HasAttachedIntel => _attachedIntelCard != null;
        private readonly List<GameObject> _spawnedSlots = new();

        public void Setup(Person person, UIManager uiManager)
        {
            _person = person;
            _uiManager = uiManager;

            if (personNameText != null)
                personNameText.text = person.SOReference.Name;

            if (closeButton != null)
                closeButton.onClick.AddListener(() => _uiManager.DismissCommandCard());

            if (actionButton != null)
                actionButton.onClick.AddListener(OnActionButtonClicked);

            UpdateTextArea(null, null);

            if (person.IsOccupied())
            {
                _isLocked = true;
                ShowLockedState();
            }
            else
            {
                _isLocked = false;
                SetActionButton("Assign Intel", false);
            }
        }

        private void ShowLockedState()
        {
            var command = _person.Command;
            if (command == null) return;

            if (command.verb != null)
            {
                var slotGo = Instantiate(intelSlotUIPrefab, slotGroup);
                var slot = slotGo.GetComponent<IntelSlotUI>();
                if (slot != null)
                    slot.ShowStatus(command.verb.CommandType.ToString(), command.GetPercentageLeft());
                _spawnedSlots.Add(slotGo);
            }

            if (command.readyToExecute)
                SetActionButton("Get Result", true);
            else
                SetActionButton("Cancel", true);
        }

        public void OnDrop(PointerEventData eventData)
        {
            // Handled by IntelSlotUI; this prevents fall-through
        }

        public void PopulateSlots(SOIntel soIntel)
        {
            ClearSlots();

            if (_person.IsAssistant())
            {
                var verbs = soIntel.GetAvailableVerbs();
                foreach (var verb in verbs)
                {
                    if (_person.HasCompletedCommand(soIntel, verb.CommandType))
                        continue;

                    var slotGo = Instantiate(intelSlotUIPrefab, slotGroup);
                    var slot = slotGo.GetComponent<IntelSlotUI>();
                    if (slot != null)
                        slot.Setup(verb, _person, _uiManager, this);
                    _spawnedSlots.Add(slotGo);
                }
            }
            else
            {
                if (_person.HasCompletedCommand(soIntel, CommandType.None))
                    return;

                var slotGo = Instantiate(intelSlotUIPrefab, slotGroup);
                var slot = slotGo.GetComponent<IntelSlotUI>();
                if (slot != null)
                    slot.SetupNoVerb(_person, _uiManager, this);
                _spawnedSlots.Add(slotGo);
            }
        }

        public void AutoAssignIntel(IntelCardUI intelCard)
        {
            PopulateSlots(intelCard.GetIntel());

            if (_spawnedSlots.Count > 0)
            {
                var firstSlot = _spawnedSlots[0].GetComponent<IntelSlotUI>();
                if (firstSlot != null)
                    firstSlot.AcceptIntel(intelCard);
            }
        }

        public void OnIntelRemovedFromSlot()
        {
            _attachedIntelCard = null;
            if (_person != null && _person.Command != null)
            {
                _person.Command.SoIntel = null;
                _person.Command.verb = null;
            }
            UpdateTextArea(null, null);
            SetActionButton("Assign Intel", false);
        }

        public void OnIntelDroppedOnSlot(IntelCardUI intelCard, Verb verb)
        {
            _attachedIntelCard = intelCard;

            if (_person.Command != null)
            {
                _person.Command.SoIntel = intelCard.GetIntel();
                _person.Command.verb = verb;
            }

            UpdateTextArea(intelCard.GetIntel(), verb);
            SetActionButton("Execute", true);
        }

        private void OnActionButtonClicked()
        {
            if (_person.HasReadyCommand())
            {
                _uiManager.DismissCommandCard();
                _uiManager.ExecuteReadyCommand(_person);
                return;
            }

            if (_isLocked)
            {
                _uiManager.CancelCommand(_person);
                return;
            }

            _uiManager.SubmitCommand(_person);
        }

        private void UpdateTextArea(SOIntel soIntel, Verb verb)
        {
            if (textArea == null || _person == null) return;

            if (soIntel == null || verb == null)
            {
                textArea.text = _person.SOReference.Description;
                return;
            }

            // Check verb's PersonStartMessages for this person
            if (verb.personStartMessages != null)
            {
                foreach (var psm in verb.personStartMessages)
                {
                    if (psm.person == _person.SOReference)
                    {
                        textArea.text = psm.message;
                        return;
                    }
                }
            }

            // Fall back to default intel messages
            var defaults = _person.SOReference.defaultIntelMessages;
            if (defaults != null)
            {
                var msg = defaults.GetMessage(soIntel.category, verb.CommandType);
                if (!string.IsNullOrEmpty(msg))
                {
                    textArea.text = msg;
                    return;
                }
            }

            textArea.text = _person.SOReference.Description;
        }

        private void SetActionButton(string text, bool interactable)
        {
            if (actionButtonText != null)
                actionButtonText.text = text;
            if (actionButton != null)
                actionButton.interactable = interactable;
        }

        public void ClearPendingCommand()
        {
            if (_person != null && !_person.IsOccupied())
                _person.ClearCommand();
        }

        public void Cleanup()
        {
            _attachedIntelCard?.DetachFromSlot();
            _attachedIntelCard = null;

            foreach (var go in _spawnedSlots)
                Destroy(go);
            _spawnedSlots.Clear();
        }

        public void ClearSlots()
        {
            foreach (var go in _spawnedSlots)
                Destroy(go);
            _spawnedSlots.Clear();
        }
    }
}
