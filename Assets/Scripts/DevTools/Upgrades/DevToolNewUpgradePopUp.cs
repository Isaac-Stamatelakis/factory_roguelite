using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

namespace DevTools.Upgrades
{
    internal class DevToolNewUpgradePopUp : MonoBehaviour
    {
        [SerializeField] private TMP_InputField mInputField;
        [SerializeField] private Button mBackButton;
        [SerializeField] private Button mCreateButton;
        [SerializeField] private TMP_Dropdown mDropDownMenu;
        internal void Initialize(DevToolUpgradeSelector upgradeSelector)
        {
            
        }
    }
}
