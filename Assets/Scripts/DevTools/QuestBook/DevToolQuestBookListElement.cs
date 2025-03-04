using System;
using System.IO;
using Newtonsoft.Json;
using Robot.Upgrades;
using Robot.Upgrades.Network;
using TMPro;
using UI;
using UI.NodeNetwork;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace DevTools.Upgrades
{
    public class DevToolQuestBookListElement : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI mPathText;
        private Action<string> onClick;
        private string path;
        internal void Display(string path, Action<string> onClick)
        {
            mPathText.text = Path.GetFileName(path).Replace(".bin","");
            this.onClick = onClick;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onClick?.Invoke(path);
        }
    }
}
