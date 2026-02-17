using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Hostage.SO;
using Hostage.Core;

namespace Hostage.UI
{
    public class PersonCardUI : MonoBehaviour, IPointerClickHandler, IDropHandler
    {
        public TMPro.TMP_Text personNameText;
        public Image panel;
        public RectTransform highlight;
        public RectTransform progress;
        private Person _person;
        private UIManager _uiManager;

        private static readonly Color NormalColor = Color.gray;
        private static readonly Color OccupiedColor = Color.gray2;

        public Person Person => _person;

        public void Setup(Person person, UIManager uiManager)
        {
            _person = person;
            _uiManager = uiManager;
            personNameText.text = person.SOReference.Name;
            progress.gameObject.SetActive(false);
            UpdatePersonFlag();
            SetHighlight(false);
        }

        public void UpdateProgress(float percentageLeft)
        {
            if (progress != null)
            {
                progress.gameObject.SetActive(true);
                progress.localScale = new Vector3(1f, percentageLeft, 1f);
            }
        }

        public void UpdatePersonFlag()
        {
            if (panel != null)
                panel.color = _person.IsOccupied() ? OccupiedColor : NormalColor;

            personNameText.text = _person.IsIdentified() ? _person.SOReference.Name : (string.IsNullOrEmpty(_person.SOReference.UnknownName)?"Unknown":_person.SOReference.UnknownName);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.dragging) return;
            _uiManager.ShowCommandCard(_person);
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!highlight.gameObject.activeSelf) return;

            var intelCard = eventData.pointerDrag?.GetComponent<IntelCardUI>();
            if (intelCard == null) return;

            _uiManager.ShowCommandCardWithIntel(_person, intelCard);
            SetHighlight(false);
        }

        public void SetHighlight(bool highlighted)
        {
            highlight.gameObject.SetActive(highlighted);
        }
    }
}
