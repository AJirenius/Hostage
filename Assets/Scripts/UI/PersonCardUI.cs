using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Hostage.SO;
using Hostage.Core;

namespace Hostage.UI
{
    public class PersonCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public TMPro.TMP_Text personNameText;
        public TMPro.TMP_Text descriptionText;
        public GameObject commandPrefab;
        private Person _person;
        private UIManager _uiManager;
        private readonly List<GameObject> _spawnedButtons = new();

        public void Setup(Person person, UIManager uiManager)
        {
            _person = person;
            _uiManager = uiManager;
            personNameText.text = person.SOReference.Name;
            descriptionText.text = person.SOReference.Description;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            var intelCard = eventData.pointerDrag?.GetComponent<IntelCardUI>();
            if (intelCard == null) return;

            _uiManager.ClearAllCommandButtons();
            ShowCommandButtons(intelCard.GetIntel());
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ClearCommandButtons();
        }

        private void ShowCommandButtons(Intel intel)
        {
            var verbs = GetAvailableVerbs(intel);
            float offsetX = 0f;

            foreach (var verb in verbs)
            {
                var buttonGo = Instantiate(commandPrefab, transform);
                buttonGo.transform.SetAsLastSibling();
                buttonGo.transform.localPosition = new Vector3(100f, offsetX, 0f);

                var commandButton = buttonGo.GetComponent<CommandButtonUI>();
                if (commandButton != null)
                    commandButton.Setup(verb, _person, _uiManager);

                _spawnedButtons.Add(buttonGo);
                offsetX += 100f;
            }
        }

        public void ClearCommandButtons()
        {
            foreach (var go in _spawnedButtons)
                Destroy(go);
            _spawnedButtons.Clear();
        }

        private List<Verb> GetAvailableVerbs(Intel intel)
        {
            var verbs = new List<Verb>();
            if (intel.investigate is { isAvailable: true }) verbs.Add(intel.investigate);
            if (intel.interview is { isAvailable: true }) verbs.Add(intel.interview);
            if (intel.surveillance is { isAvailable: true }) verbs.Add(intel.surveillance);
            if (intel.analyze is { isAvailable: true }) verbs.Add(intel.analyze);
            return verbs;
        }
    }
}
