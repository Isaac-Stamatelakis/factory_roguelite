using System.Collections.Generic;
using System.IO;
using DevTools.CraftingTrees.Network;
using Newtonsoft.Json;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using WorldModule;

namespace DevTools.CraftingTrees.Selector
{
    public class NewCraftingTreePopUpUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI mTextElement;
        [SerializeField] private TMP_InputField mInputField;
        [SerializeField] private Button mCreateButton;
        [SerializeField] private DynamicColorTextUI mDynamicColorTextUI;
        [SerializeField] private Button mBackButton;
        internal void Initialize(CraftingTreeSelectorUI craftingTreeSelectorUI)
        {
            mBackButton.onClick.AddListener(() =>
            {
                GameObject.Destroy(gameObject);
            });
            mDynamicColorTextUI.init(
                colors: DynamicTextColorFactory.getRainbow(),
                positions: DynamicTextPositionFactory.getWave(),
                "CREATE",
                10
            );
            
            mCreateButton.onClick.AddListener(CreateNew);
            return;
            
            void CreateNew()
            {
                string upgradeName = mInputField.text;
                if (string.IsNullOrEmpty(upgradeName)) return;

                SerializedCraftingTreeNodeNetwork nodeNetwork = new SerializedCraftingTreeNodeNetwork
                {
                    SerializedNodes = new List<SerializedNodeData>()
                };
                string folderPath = DevToolUtils.GetDevToolPath(DevTool.CraftingTree);
                string path = Path.Combine(folderPath,upgradeName) + ".bin";
                GlobalHelper.SerializeCompressedJson(nodeNetwork, path);
                craftingTreeSelectorUI.DisplayList();
                GameObject.Destroy(gameObject);
            }
            
        }
    }
}
