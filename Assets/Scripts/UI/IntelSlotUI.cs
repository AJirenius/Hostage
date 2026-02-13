using UnityEngine;
using UnityEngine.UI;
using Hostage.Core;
using Hostage.SO;

namespace Hostage.UI
{
    public class IntelSlotUI : MonoBehaviour
    {
        public TMPro.TMP_Text titleText;
        public RectTransform container;
        public Button button;

        private Verb _verb;
        private Person _person;
        private SOIntel _soIntel;
        private UIManager _uiManager;
        private bool _hasVerb;

        public void Setup(Verb verb, Person person, SOIntel soIntel, UIManager uiManager)
        {
            _verb = verb;
            _person = person;
            _soIntel = soIntel;
            _uiManager = uiManager;
            _hasVerb = true;

            if (titleText != null)
                titleText.text = verb.CommandType.ToString();

            if (button != null)
                button.onClick.AddListener(OnSlotClicked);
        }

        public void SetupNoVerb(Person person, SOIntel soIntel, UIManager uiManager)
        {
            _person = person;
            _soIntel = soIntel;
            _uiManager = uiManager;
            _hasVerb = false;

            if (titleText != null)
                titleText.text = "?";

            if (button != null)
                button.onClick.AddListener(OnSlotClicked);
        }

        private void OnSlotClicked()
        {
            if (_hasVerb)
                _uiManager.OnVerbSelected(_verb, _person, _soIntel);
            else
                _uiManager.OnButtonSelected(_person, _soIntel);
        }
    }
}
