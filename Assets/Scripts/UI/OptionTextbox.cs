using System;
using UnityEngine;
using UnityEngine.UI;

namespace Hostage.UI
{
    public class OptionTextbox : MonoBehaviour
    {
        public TMPro.TMP_Text textArea;

        private Action _onClicked;

        public void Setup(string message, Action onClicked)
        {
            textArea.text = message;
            _onClicked = onClicked;
            GetComponent<Button>().onClick.AddListener(OnButtonClicked);
        }

        private void OnButtonClicked()
        {
            var callback = _onClicked;
            _onClicked = null;
            callback?.Invoke();
        }
    }
}
