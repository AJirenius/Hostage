using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Hostage.SO;

namespace Hostage.UI
{
    public class IntelCardUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Text intelNameText;
        public Text descriptionText;
        private Intel _intel;
        private UIManager _uiManager;
        private CanvasGroup _canvasGroup;
        private RectTransform _rectTransform;
        private Vector3 _startPosition;

        public void Setup(Intel intel, UIManager uiManager)
        {
            _intel = intel;
            _uiManager = uiManager;
            intelNameText.text = intel.intelName;
            descriptionText.text = intel.description;
            _canvasGroup = GetComponent<CanvasGroup>();
            _rectTransform = GetComponent<RectTransform>();
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
        }

        public Intel GetIntel() => _intel;
    }
}
