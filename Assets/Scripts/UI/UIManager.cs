using System.Collections.Generic;
using Core;
using UnityEngine;
using DefaultNamespace;
using SO;

namespace UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Prefabs")] public GameObject intelCardPrefab;
        public GameObject personCardPrefab;
        [Header("Parents")] public Transform intelParent;
        public Transform personParent;

        private PlayerInventory _playerInventory;
        private List<ActionPerson> _persons;
        private ActionManager _actionManager;

        public void Initialize(PlayerInventory inventory, List<ActionPerson> persons, ActionManager actionManager)
        {
            _playerInventory = inventory;
            _persons = persons;
            _actionManager = actionManager;
            PopulateIntelCards();
            PopulatePersonCards();
        }

        private void PopulateIntelCards()
        {
            foreach (var intel in _playerInventory.GetAllIntel())
            {
                var card = Instantiate(intelCardPrefab, intelParent);
                var intelCard = card.GetComponent<IntelCardUI>();
                intelCard.Setup(intel, this);
            }
        }

        private void PopulatePersonCards()
        {
            foreach (var person in _persons)
            {
                var card = Instantiate(personCardPrefab, personParent);
                var personCard = card.GetComponent<PersonCardUI>();
                personCard.Setup(person, this);
            }
        }

        // Called by IntelCardUI when dropped on a PersonCardUI
        public void OnIntelDroppedOnPerson(Intel intel, ActionPerson person)
        {
            // Pick first available verb for prototype
            Verb verb = intel.investigate ?? intel.interview ?? (Verb)intel.surveillance ?? intel.analyze;
            if (verb == null) return;
            var action = new Action(verb, person);
            _actionManager.AddAction(action);
        }
    }
}
