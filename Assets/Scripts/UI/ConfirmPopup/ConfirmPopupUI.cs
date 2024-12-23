using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.ConfirmPopup
{
    public class ConfirmPopupUI : MonoBehaviour
    {
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI textField;

        public void Display(Action confirmAction, string text)
        {
            CanvasController.Instance.DisplayObject(gameObject, new List<KeyCode> { KeyCode.Escape }, hideParent:false);
            textField.text = text;
            void ConfirmPress()
            {
                confirmAction?.Invoke();
                CanvasController.Instance.PopStack();
            }

            confirmButton.onClick.AddListener(ConfirmPress);
            cancelButton.onClick.AddListener(CanvasController.Instance.PopStack);
        }

        
    }
}
