using System;
using Items.Inventory;
using PlayerModule;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PlayerInvUI
{
    public class StackedPlayerInvUIElement : MonoBehaviour
    {
        [SerializeField] private InventoryUI playerInventoryUI;
        [SerializeField] private GameObject playerInventoryContainer;

        public void Start()
        {
            PlayerInventory playerInventory = PlayerContainer.getInstance().getInventory();
            GridLayoutGroup gridLayoutGroup = playerInventoryUI.GetComponent<GridLayoutGroup>();
            int width = GetSize((int)gridLayoutGroup.cellSize.x, (int)gridLayoutGroup.spacing.x, 10)+100;
            int height = GetSize((int)gridLayoutGroup.cellSize.y, (int)gridLayoutGroup.spacing.y, 4)+50;
            RectTransform playerRectTransform = playerInventoryContainer.transform as RectTransform;
            playerRectTransform!.sizeDelta = new Vector2(0, height);
            RectTransform rectTransform = transform as RectTransform;
            rectTransform!.sizeDelta = new Vector2(width, 0);
            playerInventoryUI.DisplayInventory(playerInventory.Inventory);
        }

        private int GetSize(int gridSize, int spacing, int count)
        {
            return gridSize * count + spacing * (count - 1);
        }

        public void DisplayWithPlayerInventory(GameObject uiObject, bool below)
        {
            uiObject.transform.SetParent(transform,false);
            if (!below)
            {
                uiObject.transform.SetAsFirstSibling();
            }
        }

        public void SetBackgroundColor(Color color)
        {
            GetComponent<Image>().color = color;
        }

        
    }
}
