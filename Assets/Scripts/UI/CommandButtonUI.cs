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
        private bool _hasVerb;
        public Image panel;

        public void Setup(Verb verb, Person person, UIManager uiManager, SOIntel soIntel)
        {
            _verb = verb;
            _person = person;
            _uiManager = uiManager;
            _soIntel = soIntel;
            _hasVerb = true;

            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0.5f;

            var label = GetComponentInChildren<TMPro.TMP_Text>();
            if (label != null)
                label.text = _verb.CommandType.ToString();
        }

        public void SetupNoVerb(Person person, UIManager uiManager, SOIntel soIntel)
        {
            _person = person;
            _uiManager = uiManager;
            _soIntel = soIntel;
            _hasVerb = false;

            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            _canvasGroup.alpha = 0.5f;

            var label = GetComponentInChildren<TMPro.TMP_Text>();
            if (label != null)
                label.text = "?";
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

            if (_hasVerb)
                _uiManager.OnVerbSelected(_verb, _person, _soIntel);
            else
                _uiManager.OnButtonSelected(_person, _soIntel);
        }
    }
}
