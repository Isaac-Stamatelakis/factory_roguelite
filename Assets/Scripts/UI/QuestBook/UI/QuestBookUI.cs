using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.QuestBook {
    public class QuestBookUI : MonoBehaviour
    {
        [SerializeField] private bool editMode = true;
        [SerializeField] private Transform nodeContainer;
        [SerializeField] private Transform lineContainer;
        [SerializeField] private Transform contentContainer;
        private string editModePath = "UI/Quest/EditModeElements";
        private QuestEditModeController editModeController;

        public Transform NodeContainer { get => nodeContainer;}
        public Transform LineContainer { get => lineContainer;}
        public Transform ContentContainer {get => contentContainer;}

        // Start is called before the first frame update
        void Start()
        {
            if (editMode) {
                initEditMode();
            }   
        }

        private void initEditMode() {
            GameObject prefab = Resources.Load<GameObject>(editModePath);
            if (prefab == null) {
                Debug.LogError("QuestBookUI edit mode prefab is null");
                return;
            }
            GameObject instianated = GameObject.Instantiate(prefab);
            editModeController = instianated.GetComponent<QuestEditModeController>();
            editModeController.transform.SetParent(transform,false);
            editModeController.init(this);
            
        }
    }
}

