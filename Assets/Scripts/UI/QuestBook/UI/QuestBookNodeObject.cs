using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ItemModule;

namespace UI.QuestBook {
    public class QuestBookNodeObject : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private Button button;
        private QuestBookNode node;
        
        private void init(QuestBookNode node) {
            this.node = node;
            ItemObject itemObject = ItemRegistry.getInstance().getItemObject(node.ItemImageID);
            if (itemObject != null) {
                image.sprite = itemObject.getSprite();
            }
            button.onClick.AddListener(openContent);
        }
        public void OnDestroy() {
            button.onClick.RemoveAllListeners();
        }
        private void openContent() {
            
        }
    }
}

