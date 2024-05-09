using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UI;

namespace DevTools.Structures {
    public class NewStructurePopUpUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField nameField;
        [SerializeField] private Button backButton;
        [SerializeField] private Button createButton;
        [SerializeField] private Transform dynamicTextContainer;
        [SerializeField] private DynamicColorTextUI dynamicColorTextPrefab;
        private StructureDevControllerUI controllerUI;

        public void init(StructureDevControllerUI controllerUI) {
            this.controllerUI = controllerUI;
            DynamicColorTextUI dynamicColorTextUI = GameObject.Instantiate(dynamicColorTextPrefab);
            dynamicColorTextUI.init(
                colors: DynamicTextColorFactory.getRainbow(),
                positions: DynamicTextPositionFactory.getWave(),
                "CREATE",
                10
            );
            
            
            dynamicTextContainer.localScale = new Vector3(1.5f,1.5f,1f);
            dynamicColorTextUI.transform.SetParent(dynamicTextContainer.transform,false);
            backButton.onClick.AddListener(() => {
                GameObject.Destroy(gameObject);
            });
            createButton.onClick.AddListener(() => {
                if (nameField.text.Length == 0) {
                    nameField.placeholder.color = Color.red;
                    return;
                }
                
                StructureGeneratorHelper.newStructure(nameField.text);
                controllerUI.displayList();
                GameObject.Destroy(gameObject);
            });

        }

        public static NewStructurePopUpUI newInstance() {
            return AddressableLoader.getPrefabComponentInstantly<NewStructurePopUpUI>("Assets/UI/DevTools/Structure/NewStructurePopUp.prefab");
        }
    }
}

