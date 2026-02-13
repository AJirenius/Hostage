using System.Collections.Generic;
using Hostage.Core;
using Hostage.SO;
using UnityEngine;
using UnityEngine.UI;

namespace Hostage.UI
{
    public class CommandCardUI : MonoBehaviour
    {
        public RectTransform slotGroup;
        public TMPro.TMP_Text personNameText;
        public Image profileIcon;
        public GameObject intelSlotUIPrefab;

        private Person _person;
        private SOIntel _soIntel;
        private UIManager _uiManager;
        private readonly List<GameObject> _spawnedSlots = new();

        public void Setup(Person person, SOIntel soIntel, UIManager uiManager)
        {
            _person = person;
            _soIntel = soIntel;
            _uiManager = uiManager;

            if (personNameText != null)
                personNameText.text = person.SOReference.Name;

            PopulateSlots();
        }

        private void PopulateSlots()
        {
            if (_person.IsAssistant())
            {
                var verbs = GetAvailableVerbs(_soIntel);
                foreach (var verb in verbs)
                {
                    var slotGo = Instantiate(intelSlotUIPrefab, slotGroup);
                    var slot = slotGo.GetComponent<IntelSlotUI>();
                    if (slot != null)
                        slot.Setup(verb, _person, _soIntel, _uiManager);
                    _spawnedSlots.Add(slotGo);
                }
            }
            else
            {
                var slotGo = Instantiate(intelSlotUIPrefab, slotGroup);
                var slot = slotGo.GetComponent<IntelSlotUI>();
                if (slot != null)
                    slot.SetupNoVerb(_person, _soIntel, _uiManager);
                _spawnedSlots.Add(slotGo);
            }
        }

        private List<Verb> GetAvailableVerbs(SOIntel soIntel)
        {
            var verbs = new List<Verb>();
            if (soIntel.investigate is { isAvailable: true }) verbs.Add(soIntel.investigate);
            if (soIntel.interview is { isAvailable: true }) verbs.Add(soIntel.interview);
            if (soIntel.surveillance is { isAvailable: true }) verbs.Add(soIntel.surveillance);
            if (soIntel.analyze is { isAvailable: true }) verbs.Add(soIntel.analyze);
            return verbs;
        }

        public void Cleanup()
        {
            foreach (var go in _spawnedSlots)
                Destroy(go);
            _spawnedSlots.Clear();
        }
    }
}
