using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Hostage.Core;
using Hostage.SO;

namespace Hostage.UI
{
    public class IntelSlotUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public TMPro.TMP_Text titleText;
        public RectTransform container;
        public Button button;

        private Verb _verb;
        private Person _person;
        private UIManager _uiManager;
        private CommandCardUI _commandCard;
        private bool _hasVerb;

        public void Setup(Verb verb, Person person, UIManager uiManager, CommandCardUI commandCard)
        {
            _verb = verb;
            _person = person;
            _uiManager = uiManager;
            _commandCard = commandCard;
            _hasVerb = true;

            if (titleText != null)
                titleText.text = verb.CommandType.ToString();
        }

        public void SetupNoVerb(Person person, UIManager uiManager, CommandCardUI commandCard)
        {
            _person = person;
            _uiManager = uiManager;
            _commandCard = commandCard;
            _hasVerb = false;

            if (titleText != null)
                titleText.text = "?";
        }

        public void ShowStatus(string label, float percentageLeft)
        {
            if (titleText != null)
                titleText.text = label;

            if (button != null)
                button.interactable = false;
        }

        public void OnDrop(PointerEventData eventData)
        {
            var intelCard = eventData.pointerDrag?.GetComponent<IntelCardUI>();
            if (intelCard == null) return;

            AcceptIntel(intelCard);
        }

        public void AcceptIntel(IntelCardUI intelCard)
        {
            intelCard.AttachToSlot(container, _commandCard);
            _commandCard.OnIntelDroppedOnSlot(intelCard, _verb);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            var intelCard = eventData.pointerDrag?.GetComponent<IntelCardUI>();
            if (intelCard == null) return;

            // TODO: highlight slot visual
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // TODO: remove highlight
        }
    }
}
