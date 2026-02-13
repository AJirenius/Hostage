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

            string intelLabel = command.SoIntel != null ? command.SoIntel.intelName : "";
            SetActionButton($"In Progress - {intelLabel}", false);
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

            SetActionButton("Execute", true);
        }

        private void OnActionButtonClicked()
        {
            _uiManager.SubmitCommand(_person);
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
