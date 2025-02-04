using System.Collections.Generic;
using PlayerModule.KeyPress;
using UnityEngine;

namespace UI.QuestBook
{
    public class QuestBookUIManager : MonoBehaviour
    {
        [SerializeField] private QuestBookSelectorUI selectorPrefab;
        private static QuestBookUIManager instance;
        public static QuestBookUIManager Instance => instance;

        public void Awake()
        {
            instance = this;
        }

        public void Initialize(QuestBookLibrary library)
        {
            GlobalHelper.deleteAllChildren(transform);
            QuestBookSelectorUI selectorUI = Instantiate(selectorPrefab, transform, false);
            selectorUI.Initialize(library);
            selectorUI.gameObject.SetActive(false);
        }
        void Update()
        {
            if (Input.GetKeyDown(KeyCode.L) && !PlayerKeyPressUtils.BlockKeyInput && transform.childCount > 0)
            {
                Transform child = transform.GetChild(0);
                child.gameObject.SetActive(true);
                CanvasController.Instance.DisplayObject(child.gameObject,keyCodes: new List<KeyCode> { KeyCode.L}, hideParent:false, originalParent:transform, blocker:true);
            }
        }
        
    }
}
