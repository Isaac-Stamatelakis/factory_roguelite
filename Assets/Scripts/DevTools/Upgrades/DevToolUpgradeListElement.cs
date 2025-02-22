using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace DevTools.Upgrades
{
    public class DevToolUpgradeListElement : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI mNameText;
        internal void Display(DevToolUpgradeInfo upgradeInfo)
        {
            mNameText.text = Path.GetDirectoryName(upgradeInfo.Path);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            
        }
    }
}
