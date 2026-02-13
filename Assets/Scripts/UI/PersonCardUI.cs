using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Hostage.SO;
using Hostage.Core;

namespace Hostage.UI
{
    public class PersonCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler
    {
        public TMPro.TMP_Text personNameText;
        public TMPro.TMP_Text descriptionText;
        public Image panel;
        public RectTransform progress;
        private Person _person;
        private UIManager _uiManager;

        private static readonly Color NormalColor = Color.gray;
        private static readonly Color OccupiedColor = Color.red;

        public Person Person => _person;

        public void Setup(Person person, UIManager uiManager)
        {
            _person = person;
            _uiManager = uiManager;
            personNameText.text = person.SOReference.Name;
            UpdatePersonFlag();
        }

        public void UpdateProgress(float percentageLeft)
        {
            //if (progress != null)
              // show progress
        }

        public void UpdatePersonFlag()
        {
            if (panel != null)
                panel.color = _person.IsOccupied() ? OccupiedColor : NormalColor;

            personNameText.text = _person.IsUnknown() ? (string.IsNullOrEmpty(_person.SOReference.UnknownName)?"Unknown":_person.SOReference.UnknownName) : _person.SOReference.Name;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_person.IsOccupied()) return;

            var intelCard = eventData.pointerDrag?.GetComponent<IntelCardUI>();
            if (intelCard == null) return;

            SetHighlight(true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            SetHighlight(false);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (_person.IsOccupied()) return;

            var intelCard = eventData.pointerDrag?.GetComponent<IntelCardUI>();
            if (intelCard == null) return;

            SetHighlight(false);
            _uiManager.ShowCommandCard(_person, intelCard.GetIntel());
        }

        private void SetHighlight(bool highlighted)
        {
            // TODO: highlight border visual
        }
    }
}
