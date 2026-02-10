using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hostage.Core;
using Hostage.SO;

namespace Hostage.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Prefabs")] public GameObject intelCardPrefab;
        public GameObject personCardPrefab;
        [Header("Parents")] public Transform intelParent;
        public Transform personParent;

        private PlayerInventory _playerInventory;
        private ActionManager _actionManager;
        private PersonManager _personManager;
        private Dictionary<Intel, GameObject > _createdIntelCards = new Dictionary<Intel, GameObject>();
        private Dictionary<Person, GameObject > _createdPersonCards = new Dictionary<Person, GameObject>();
        private float _spawnX = -200;
        public void Initialize(PlayerInventory inventory, ActionManager actionManager, PersonManager personManager)
        {
            _playerInventory = inventory;
            _actionManager = actionManager;
            _personManager = personManager;
            RefreshIntelCards();
            RefreshPersonCards();
        }

        private void RefreshIntelCards()
        {
            IReadOnlyList<Intel> inventoryIntels = _playerInventory.GetAllIntel();
            var keysToRemove = new List<Intel>();
            foreach (KeyValuePair<Intel, GameObject> kvp in _createdIntelCards)
            {
                if (!inventoryIntels.Contains(kvp.Key))
                {
                    Destroy(kvp.Value);
                    keysToRemove.Add(kvp.Key);
                }
            }
            foreach (var key in keysToRemove)
            {
                Debug.LogWarning("key:"+key+" was removed from inventory but still had a card. Removing card.");
                _createdIntelCards.Remove(key);
            }
            foreach (var intel in inventoryIntels)
            {
                
                if (!_createdIntelCards.ContainsKey(intel))
                {
                    var card = Instantiate(intelCardPrefab, intelParent);
                    var intelCard = card.GetComponent<IntelCardUI>();
                    intelCard.Setup(intel, this);
                    card.transform.localPosition += new Vector3(_spawnX, Random.Range(-50, 50), 0);
                    _createdIntelCards[intel] = card;
                    _spawnX += 200;
                    if (_spawnX > 800) _spawnX = -200; // reset position if too many cards
                }
            }
        }

        private void RefreshPersonCards()
        {
            // Remove old cards
            foreach (var kvp in _createdPersonCards)
            {
                Destroy(kvp.Value);
            }
            _createdPersonCards.Clear();

            float x = 0;
            var persons = _personManager.GetAllPersons();
            foreach (var person in persons)
            {
                if (!person.IsAvailable())
                    continue;
                var card = Instantiate(personCardPrefab, personParent);
                var personCard = card.GetComponent<PersonCardUI>();
                personCard.Setup(person, this);
                card.transform.localPosition += new Vector3(x, Random.Range(-50, 50), 0);
                _createdPersonCards[person] = card;
                x += 200;
            }
        }

        // Called by IntelCardUI when dropped on a PersonCardUI
        public void OnIntelDroppedOnPerson(Intel intel, Person person)
        {
            Verb verb = intel.investigate ?? intel.interview ?? (Verb)intel.surveillance ?? intel.analyze;
            if (verb == null) return;
            var action = new Action(verb, person.SOReference);
            _actionManager.AddAction(action);
        }
    }
}
