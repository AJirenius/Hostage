using UnityEngine;
using UnityEngine.EventSystems;
using Hostage.Core;
using Hostage.SO;
using UnityEngine.UI;

namespace Hostage.UI
{
    public class CommandButtonUI : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private Verb _verb;
        private Person _person;
        private UIManager _uiManager;
        private SOIntel _soIntel;
        private CanvasGroup _canvasGroup;
        public Image panel;

        public void Setup(Verb verb, Person person, UIManager uiManager, SOIntel soIntel)
        {
            _verb = verb;
            _person = person;
            _uiManager = uiManager;
            _soIntel = soIntel;

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
            panel.color = Color.green;
            
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _canvasGroup.alpha = 0.5f;
            panel.color = Color.grey;
        }

        public void OnDrop(PointerEventData eventData)
        {
            var intelCard = eventData.pointerDrag?.GetComponent<IntelCardUI>();
            if (intelCard == null) return;

            _uiManager.OnVerbSelected(_verb, _person, _soIntel);
        }
    }
}
