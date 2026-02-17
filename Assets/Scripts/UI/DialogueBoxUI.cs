using System;
using UnityEngine;

namespace Hostage.UI
{
    public class DialogueBoxUI : MonoBehaviour
    {
        public TMPro.TMP_Text personNameText;
        public TMPro.TMP_Text textArea;
        public UnityEngine.UI.Image profileIcon;

        private Action _onDismissed;

        public void Show(string speakerName, string message, Action onDismissed)
        {
            personNameText.text = speakerName;
            textArea.text = message;
            _onDismissed = onDismissed;
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                var callback = _onDismissed;
                _onDismissed = null;
                Destroy(gameObject);
                callback?.Invoke();
            }
        }
    }
}
