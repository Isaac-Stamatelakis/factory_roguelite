using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Catalogue.InfoViewer
{
    public class CatalogueNavigator : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI title;
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;

        public void Initialize(string titleText, Action onLeftClick, Action onRightClick)
        {
            this.title.text = titleText;
            this.leftButton.onClick.AddListener(() => onLeftClick?.Invoke());
            this.rightButton.onClick.AddListener(() => onRightClick?.Invoke());
        }
    
        public void SetText(string text) => title.text = text;
    
    }
}
