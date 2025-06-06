using System.Collections;
using System.Collections.Generic;
using Item.Slot;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Items;
using UnityEngine.EventSystems;
using Items.Inventory;
using UI.QuestBook.Data.Node;

namespace UI.QuestBook {
    public class ConnectionElementUI : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image mImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI idText;
        [SerializeField] private Image panel;
        private QuestBookNodeData node;
        private List<int> nodeConnections;
        
        public void Initialize(List<int> nodeConnections, QuestBookNodeData node) {
            this.node = node;
            this.nodeConnections = nodeConnections;
            ItemObject itemObject = ItemRegistry.GetInstance().GetItemObject(node.ImageSeralizedItemSlot?.id);
            mImage.sprite = itemObject?.GetSprite();
            nameText.text = node.Content.Title;
            idText.text = "#" + node.Id.ToString();
            setColor();
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

