using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace DevTools.Structures {
    public class EditStructurePopUpUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField editTitle;
        [SerializeField] private Button deleteButton;
        [SerializeField] private Image deleteButtonPanel;
        [SerializeField] private Button backButton;

        private float timeSinceLastDeletePress = 100f;

        private StructureSelectorUI listElement;

        public void Update() {
            timeSinceLastDeletePress += Time.deltaTime;
            if (timeSinceLastDeletePress <= 1f) {
                deleteButtonPanel.color = Color.red;
            } else {
                deleteButtonPanel.color = Color.white;
            }
        }
        public void init(StructureSelectorUI listElement) {
            editTitle.text = listElement.getTitle();
            this.listElement = listElement;
            backButton.onClick.AddListener(() => {
                listElement.setTitle(editTitle.text);
                GameObject.Destroy(gameObject);
            });
            deleteButton.onClick.AddListener(() => {
                if (timeSinceLastDeletePress <= 1f) {
                    listElement.deleteSelf();
                    GameObject.Destroy(gameObject);
                }
                timeSinceLastDeletePress = 0f;
            });
        }

        public void goBack() {
            
        }
        public static EditStructurePopUpUI newInstance() {
            return AddressableLoader.getPrefabComponentInstantly<EditStructurePopUpUI>("Assets/UI/DevTools/Structure/EditStructurePopUp.prefab");
        }
    }
}

