<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> 56642417 (Added crafting tree data structures, item slot uis now constantly update tooltip when focused)
using System.Collections.Generic;
using System.IO;
using DevTools.CraftingTrees.Network;
using Newtonsoft.Json;
<<<<<<< HEAD
=======
>>>>>>> 286e87ab (Added crafting tree dev tool prep, catalgoue control navigator now displays itemslots instead of images)
=======
>>>>>>> 56642417 (Added crafting tree data structures, item slot uis now constantly update tooltip when focused)
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
<<<<<<< HEAD
<<<<<<< HEAD
using WorldModule;
=======
>>>>>>> 286e87ab (Added crafting tree dev tool prep, catalgoue control navigator now displays itemslots instead of images)
=======
using WorldModule;
>>>>>>> 56642417 (Added crafting tree data structures, item slot uis now constantly update tooltip when focused)

namespace DevTools.CraftingTrees.Selector
{
    public class NewCraftingTreePopUpUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI mTextElement;
<<<<<<< HEAD
<<<<<<< HEAD
        [SerializeField] private TMP_InputField mInputField;
=======
>>>>>>> 286e87ab (Added crafting tree dev tool prep, catalgoue control navigator now displays itemslots instead of images)
=======
        [SerializeField] private TMP_InputField mInputField;
>>>>>>> 56642417 (Added crafting tree data structures, item slot uis now constantly update tooltip when focused)
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
<<<<<<< HEAD
<<<<<<< HEAD
=======

>>>>>>> 286e87ab (Added crafting tree dev tool prep, catalgoue control navigator now displays itemslots instead of images)
=======
>>>>>>> 56642417 (Added crafting tree data structures, item slot uis now constantly update tooltip when focused)
            
            mCreateButton.onClick.AddListener(CreateNew);
            return;
            
            void CreateNew()
            {
<<<<<<< HEAD
<<<<<<< HEAD
=======
>>>>>>> 56642417 (Added crafting tree data structures, item slot uis now constantly update tooltip when focused)
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
<<<<<<< HEAD
=======
                
>>>>>>> 286e87ab (Added crafting tree dev tool prep, catalgoue control navigator now displays itemslots instead of images)
=======
>>>>>>> 56642417 (Added crafting tree data structures, item slot uis now constantly update tooltip when focused)
            }
            
        }
    }
}
