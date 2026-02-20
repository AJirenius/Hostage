using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hostage.UI
{
    public class DialogueBoxUI : MonoBehaviour
    {
        public TMPro.TMP_Text personNameText;
        public TMPro.TMP_Text textArea;
        public UnityEngine.UI.Image profileIcon;
        public GameObject optionPrefab;
        public Transform optionContainer;

        private Action _onDismissed;
        private bool _isChoiceMode;

        public void Show(string speakerName, string message, Action onDismissed)
        {
            personNameText.text = speakerName;
            textArea.text = message;
            _onDismissed = onDismissed;
            _isChoiceMode = false;
        }

        public void ShowChoice(string speakerName, string message, List<string> options, Action<int> onOptionSelected)
        {
            personNameText.text = speakerName;
            textArea.text = message;
            _isChoiceMode = true;

            for (int i = 0; i < options.Count; i++)
            {
                int index = i;
                var go = Instantiate(optionPrefab, optionContainer);
                go.GetComponent<OptionTextbox>().Setup(options[i], () =>
                {
                    Destroy(gameObject);
                    onOptionSelected?.Invoke(index);
                });
            }
        }

        private void Update()
        {
            if (_isChoiceMode) return;

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
