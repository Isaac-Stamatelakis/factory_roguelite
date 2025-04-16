using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace DevTools.CraftingTrees.Selector
{
    public class NewCraftingTreePopUpUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI mTextElement;
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
                
            }
            
        }
    }
}
