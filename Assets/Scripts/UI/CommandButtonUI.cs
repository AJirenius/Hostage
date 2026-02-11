using UnityEngine;
using UnityEngine.EventSystems;
using Hostage.Core;
using Hostage.SO;

namespace Hostage.UI
{
    public class CommandButtonUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private Verb _verb;
        private Person _person;
        private UIManager _uiManager;
        private Intel _intel;
        private CanvasGroup _canvasGroup;

        public void Setup(Verb verb, Person person, UIManager uiManager, Intel intel)
        {
            _verb = verb;
            _person = person;
            _uiManager = uiManager;
            _intel = intel;

            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0.5f;

            var label = GetComponentInChildren<TMPro.TMP_Text>();
            if (label != null)
                label.text = _verb.actionType.ToString();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _canvasGroup.alpha = 1f;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _canvasGroup.alpha = 0.5f;
        }

        public void OnDrop(PointerEventData eventData)
        {
            var intelCard = eventData.pointerDrag?.GetComponent<IntelCardUI>();
            if (intelCard == null) return;

            _uiManager.OnVerbSelected(_verb, _person, _intel);
        }
    }
}
