using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TileEntityModule.Instances.Matrix {
    public class AutoCraftPopupUI : MonoBehaviour
    {
        [SerializeField] private GridLayoutGroup itemElementContainer;
        [SerializeField] private TextMeshProUGUI requiredMemoryText;
        [SerializeField] private TextMeshProUGUI maxUsableCores;
        [SerializeField] private TMP_Dropdown craftingProcessorSelector;
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;
        [SerializeField] private TextMeshProUGUI currentProcessorMemory;
        [SerializeField] private TextMeshProUGUI currentProcessorCores;
        [SerializeField] private Button cancelButton;
        [SerializeField] private Button activateButton;

        public void init(ItemMatrixController controller, EncodedRecipe encodedRecipe) {
            
        }
        public static AutoCraftPopupUI newInstance() {
            return GlobalHelper.instantiateFromResourcePath("UI/Matrix/AutoCrafting/RecipeCraftPopup").GetComponent<AutoCraftPopupUI>();
        }
    }
}

