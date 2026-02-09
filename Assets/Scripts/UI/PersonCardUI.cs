using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Hostage.SO;

namespace Hostage.UI
{
    public class PersonCardUI : MonoBehaviour, IDropHandler
    {
        public TMPro.TMP_Text personNameText;
        public TMPro.TMP_Text descriptionText;
        private ActionPerson _person;
        private UIManager _uiManager;

        public void Setup(ActionPerson person, UIManager uiManager)
        {
            _person = person;
            _uiManager = uiManager;
            personNameText.text = person.Name;
            descriptionText.text = person.Description;
        }

        public void OnDrop(PointerEventData eventData)
        {
            var intelCard = eventData.pointerDrag?.GetComponent<IntelCardUI>();
            if (intelCard != null)
            {
                _uiManager.OnIntelDroppedOnPerson(intelCard.GetIntel(), _person);
            }
        }
    }
}
