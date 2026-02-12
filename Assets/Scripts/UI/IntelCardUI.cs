using UnityEngine;
using UnityEngine.EventSystems;

using Hostage.SO;

namespace Hostage.UI
{
    public class IntelCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public TMPro.TMP_Text intelNameText;
        public TMPro.TMP_Text descriptionText;
        private SOIntel _soIntel;
        private UIManager _uiManager;
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private Vector3 _startPosition;

        public void Setup(SOIntel soIntel, UIManager uiManager)
        {
            _soIntel = soIntel;
            _uiManager = uiManager;
            intelNameText.text = soIntel.intelName;
            descriptionText.text = soIntel.description;
            _canvasGroup = GetComponentInChildren<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;

        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _startPosition = _rectTransform.position;
            _canvasGroup.alpha = 0.6f;
            _canvasGroup.blocksRaycasts = false;
        }

        public void OnDrag(PointerEventData eventData)
        {
            _rectTransform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _rectTransform.position = _startPosition;
            _canvasGroup.alpha = 1f;
            _canvasGroup.blocksRaycasts = true;
            _uiManager.ClearAllCommandButtons();
        }

        public SOIntel GetIntel() => _soIntel;
    }
}
