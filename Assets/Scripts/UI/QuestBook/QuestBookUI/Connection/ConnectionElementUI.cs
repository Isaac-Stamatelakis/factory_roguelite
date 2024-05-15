using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items;
using UnityEngine.EventSystems;
using Items.Inventory;

namespace UI.QuestBook {
    public class ConnectionElementUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image image;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI idText;
        [SerializeField] private Image panel;
        private QuestBookNode node;
        private HashSet<int> nodeConnections;
        
        public void init(HashSet<int> nodeConnections, QuestBookNode node) {
            this.node = node;
            this.nodeConnections = nodeConnections;

            ItemObject itemObject = ItemRegistry.getInstance().getItemObject(node.ItemImageID);
            if (itemObject != null) {
                image.sprite = itemObject.getSprite();
                image.transform.localScale = ItemSlotUIFactory.getItemScale(image.sprite);
            }
            nameText.text = node.Content.Title;
            idText.text = "#" + node.Id.ToString();
            setColor();
        }
        public static ConnectionElementUI newInstance() {
            return GlobalHelper.instantiateFromResourcePath("UI/Quest/Connections/ConnectionListElement").GetComponent<ConnectionElementUI>();
        }

        private void setColor() {
            panel.color = nodeConnections.Contains(node.Id) ? Color.green : Color.white;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left) {
                if (nodeConnections.Contains(node.Id)) {
                    nodeConnections.Remove(node.Id);
                } else {
                    nodeConnections.Add(node.Id);
                }
                setColor();
            }
        }
    }
}

