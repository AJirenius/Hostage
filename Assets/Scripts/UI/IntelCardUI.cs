using UnityEngine;
using UnityEngine.EventSystems;

using Hostage.Core;
using Hostage.SO;

namespace Hostage.UI
{
    public class IntelCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public TMPro.TMP_Text intelNameText;
        private SOIntel _soIntel;
        private UIManager _uiManager;
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private Transform _originalParent;
        private Transform _dragParent;
        private CommandCardUI _attachedCommandCard;
        private bool _droppedSuccessfully;
        private bool _locked;

        public void Setup(SOIntel soIntel, UIManager uiManager)
        {
            _soIntel = soIntel;
            _uiManager = uiManager;
            intelNameText.text = GetDisplayName(soIntel);
            _canvasGroup = GetComponentInChildren<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
            _originalParent = transform.parent;
            _dragParent = GetComponentInParent<Canvas>().rootCanvas.transform;
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_locked)
            {
                eventData.pointerDrag = null;
                return;
            }

            if (_attachedCommandCard != null)
            {
                _attachedCommandCard.OnIntelRemovedFromSlot();
                _attachedCommandCard = null;
            }

            _droppedSuccessfully = false;
            transform.SetParent(_dragParent, true);
            _canvasGroup.alpha = 0.6f;
            _canvasGroup.blocksRaycasts = false;
            _uiManager.OnIntelDragStarted(_soIntel);
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!_droppedSuccessfully)
            {
                transform.SetParent(_originalParent, false);
            }
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            _uiManager.OnIntelDragEnded();
        }

        public void AttachToSlot(Transform slotContainer, CommandCardUI commandCard)
        {
            _droppedSuccessfully = true;
            _attachedCommandCard = commandCard;
            transform.SetParent(slotContainer, false);
            _rectTransform.localPosition = Vector3.zero;
        }

        public void DetachFromSlot()
        {
            _attachedCommandCard = null;
            transform.SetParent(_originalParent, false);
        }

        public void SetLocked(bool locked)
        {
            _locked = locked;
            _canvasGroup.alpha = locked ? 0.5f : 1f;
        }

        public void UpdateName(Person person)
        {
            intelNameText.text = GetDisplayName(_soIntel, person);
        }

        string GetDisplayName(SOIntel soIntel, Person person = null)
        {
            if (soIntel.category == IntelCategory.Person && soIntel.person != null)
            {
                bool isIdentified = person != null
                    ? person.IsIdentified()
                    : (soIntel.person.defaultFlag & PersonFlag.Identified) != 0;

                if (!isIdentified)
                    return soIntel.person.UnknownName;
            }
            return soIntel.intelName;
        }

        public SOIntel GetIntel() => _soIntel;
    }
}
