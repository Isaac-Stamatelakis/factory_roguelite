using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ItemModule;
using UnityEngine.EventSystems;
using UI;

namespace UI.QuestBook {
    public class QuestBookNodeObject : MonoBehaviour, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler, ILongClickable
    {
        [SerializeField] private Image image;
        [SerializeField] private Button button;
        private HoldClickInstance holdClickInstance;
        private QuestBookNode node;
        
        public void init(QuestBookNode node) {
            this.node = node;
            holdClickInstance = new HoldClickInstance(this);
            ItemObject itemObject = ItemRegistry.getInstance().getItemObject(node.ItemImageID);
            if (itemObject != null) {
                image.sprite = itemObject.getSprite();
            }
            transform.position = new Vector3(node.X,node.Y,0);
            button.onClick.AddListener(openContent);
            
        }

        public void Update() {
            if (holdClickInstance != null) {
                holdClickInstance.checkHoldStatus();
            }
            
        }

        public void longClick()
        {
            
        }

        public void OnDestroy() {
            button.onClick.RemoveAllListeners();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) {
                openContent();
            } else if (eventData.button == PointerEventData.InputButton.Right) {

            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (holdClickInstance == null) {
                return;
            }
            holdClickInstance.click();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (holdClickInstance == null) {
                return;
            }
            holdClickInstance.release();
        }

        private void openContent() {
            
        }
    }
}

